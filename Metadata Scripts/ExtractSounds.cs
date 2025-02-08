// shit that extracts sounds from banks
// https://github.com/SamboyCoding/Fmod5Sharp
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp;
using System.Text;

public class ExtractSoundAssets
{
    #region Colors because idk how to get it from the other thing
    public static string NL = Environment.NewLine; // shortcut
    public static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    public static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    public static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    public static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
    public static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
    public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    public static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
    public static string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
    public static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
    public static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
    public static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
    public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    public static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
    public static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
    #endregion

    // originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
    public static void ExtractSoundFiles(string bankPath, string outPath, string bankfilename, bool verbose)
    {
        // if Master.bank or Master.strings.bank
        if (bankPath.Contains("Master"))
            return; // ignore because it causes the thing to fail

        var bytes = File.ReadAllBytes(bankPath);
        var index = bytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("FSB5"));
        if (index > 0)
        {
            bytes = bytes.AsSpan(index).ToArray();
        }
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

            // add to xml
            List<FmodSample> samples = bank.Samples;
            // because it sometimes fails idk
            // also DEAR GOD
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
