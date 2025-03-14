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

            // add to xml
            // also try catches because it sometimes fails idk
            try
            {
                int frequency = samples[i].Metadata.Frequency; //E.g. 44100
                uint numChannels = samples[i].Metadata.Channels; //2 for stereo, 1 for mono.
                AudioFile.AudioFileXML(outPath, filePath, bankfilename, frequency, numChannels);
            }
            catch (Exception)
            {
                try
                {
                    int frequency = samples[i].Metadata.Frequency; //E.g. 44100
                    AudioFile.AudioFileXML(outPath, filePath, bankfilename, frequency, 2);
                }
                catch (Exception)
                {
                    try
                    {
                        uint numChannels = samples[i].Metadata.Channels; //2 for stereo, 1 for mono.
                        AudioFile.AudioFileXML(outPath, filePath, bankfilename, 44100, numChannels);
                    }
                    catch (Exception)
                    {
                        AudioFile.AudioFileXML(outPath, filePath, bankfilename, 44100, 2);
                    }
                }
            }
        }
    }
}
