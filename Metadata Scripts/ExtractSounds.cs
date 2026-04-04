using Fmod5Sharp.FmodTypes;
using static Program;

public class ExtractSoundAssets
{
    // Dictionary to hold all sounds and their names and extensions
    // and also check what bankfile they belong to
    public static Dictionary<string, Dictionary<string, string>> SoundsinBanks = [];

    // originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
    public static void ExtractSoundFiles(FmodSoundBank bank, string bankfilename)
    {
        string outPath = outputProjectPath + "/Assets";
        var outDir = Directory.CreateDirectory($"{outPath}/{bankfilename.Replace(".bank", "")}/");

        PushToConsoleLog($"\nExtracting Sound Files from {bankfilename}...\n", YELLOW);

        var i = 0;
        // Set up dictionary
        Dictionary<string, string> SoundNameExt = [];
        PushToConsoleLog($"Sounds Found: {bank.Samples.Count}", YELLOW);
        foreach (var bankSample in bank.Samples)
        {
            i++;
            var name = bankSample.Name ?? $"UnknownSound-{i}";

            if (!bankSample.RebuildAsStandardFileFormat(out byte[]? data, out string? extension))
            {
                PushToConsoleLog($"ERROR! - Failed to Extract Sound {name}!", RED);
                continue;
            }

            // Add Sound Name and Extension to dictionary
            SoundNameExt[name] = extension;
            // add dictionary to another dictionary specific to current bankfile
            SoundsinBanks[bankfilename] = SoundNameExt;

            var filePath = Path.Combine(outDir.FullName, $"{name}.{extension}");
            File.WriteAllBytes(filePath, data);
            PushToConsoleLog($"Extracted Sound {name}.{extension}", CYAN);

            // set defaults for frequency and channels
            int frequency = 44100;
            uint numChannels = 2;

            // get true values from sound files
            // although it fails sometimes, idk its weird
            try { frequency = bank.Samples[i]?.Metadata?.Frequency ?? 44100; } catch { }
            try { numChannels = bank.Samples[i]?.Metadata?.Channels ?? 2; } catch { }

            // add to xml
            AudioFile.AudioFileXML(outPath, filePath, frequency, numChannels);
        }
    }
}