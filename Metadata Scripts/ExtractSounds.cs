// shit that extracts sounds from banks
// https://github.com/SamboyCoding/Fmod5Sharp
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp;
using System.Text;
using static Program;

public class ExtractSoundAssets
{
    // originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
    public static void ExtractSoundFiles(string bankPath, string bankfilename)
    {
        string outPath = outputProjectPath + "/Assets";

        // if Master.bank or Master.strings.bank
        if (bankPath.Contains("Master"))
            return; // ignore because it causes the thing to fail

        var bytes = File.ReadAllBytes(bankPath);
        var index = bytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("FSB5"));

        if (index > 0)
            bytes = bytes.AsSpan(index).ToArray();

        var bank = FsbLoader.LoadFsbFromByteArray(bytes);
        var outDir = Directory.CreateDirectory(outPath + $"/{bankfilename.Replace(".bank", "")}/");

        PushToConsoleLog($"\nExtracting Sound Files from {bankfilename}...\n", YELLOW);

        var i = 0;
        foreach (var bankSample in bank.Samples)
        {
            i++;
            var name = bankSample.Name ?? $"UnknownSound-{i}";

            if (!bankSample.RebuildAsStandardFileFormat(out var data, out var extension))
            {
                PushToConsoleLog($"Failed to Extract Sound {name}", RED);
                continue;
            }

            var filePath = Path.Combine(outDir.FullName, $"{name}.{extension}");
            File.WriteAllBytes(filePath, data);
            PushToConsoleLog($"Extracted Sound {name}.{extension}", CYAN);

            List<FmodSample> samples = bank.Samples;

            // set defaults for frequency and channels
            int frequency = 44100;
            uint numChannels = 2;

            // get true values from sound files
            // although it fails sometimes, idk its weird
            try { frequency = samples[i]?.Metadata?.Frequency ?? 44100; } catch { }
            try { numChannels = samples[i]?.Metadata?.Channels ?? 2; } catch { }

            // add to xml
            AudioFile.AudioFileXML(outPath, filePath, frequency, numChannels);
        }
    }
}
