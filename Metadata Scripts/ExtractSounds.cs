// shit that extracts sounds from banks
// https://github.com/SamboyCoding/Fmod5Sharp
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp;
using System.Text;
using static Program;

public class ExtractSoundAssets
{
    // originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
    public static void ExtractSoundFiles(string bankPath, string outPath, string bankfilename, bool verbose)
    {
        // if Master.bank or Master.strings.bank
        if (bankPath.Contains("Master"))
            return; // ignore because it causes the thing to fail

        var bytes = File.ReadAllBytes(bankPath);
        var index = bytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("FSB5"));

        if (index > 0)
            bytes = bytes.AsSpan(index).ToArray();

        var bank = FsbLoader.LoadFsbFromByteArray(bytes);
        var outDir = Directory.CreateDirectory(outPath + $"/{bankfilename.Replace(".bank", "")}/");

        if (verbose)
            Console.WriteLine($"\n{YELLOW}Extracting Sound Files from {bankfilename}...{NORMAL}\n");

        var i = 0;
        foreach (var bankSample in bank.Samples)
        {
            i++;
            var name = bankSample.Name ?? $"UnknownSound-{i}";

            if (!bankSample.RebuildAsStandardFileFormat(out var data, out var extension))
            {
                Console.WriteLine($"{RED}Failed to Extract Sound {name}{NORMAL}");
                continue;
            }

            var filePath = Path.Combine(outDir.FullName, $"{name}.{extension}");
            File.WriteAllBytes(filePath, data);
            if (verbose)
                Console.WriteLine($"{CYAN}Extracted Sound {name}.{extension}{NORMAL}");

            List<FmodSample> samples = bank.Samples;

            // set defaults for frequnecy and channels
            int frequency = 44100; // default frequnecy value
            uint numChannels = 2; // 2 for stereo, 1 for mono.

            // try to get real values from bank files if possible
            // probably could've optimized this better, but idk if it's returning null or just flat out failing
            try { frequency = samples[i].Metadata.Frequency; } catch (Exception) { }
            try { numChannels = samples[i].Metadata.Channels; } catch (Exception) { }

            // add to xml
            AudioFile.AudioFileXML(outPath, filePath, frequency, numChannels);
        }
    }
}
