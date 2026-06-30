using Fmod5Sharp.FmodTypes;
using static Program;

public class ExtractSoundAssets
{
	// originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
	public static void ExtractSoundFiles(FmodSoundBank bank, string bankfilename)
	{
		string outPath = outputProjectPath + "/Assets";
		var outDir = Directory.CreateDirectory($"{outPath}/{bankfilename.Replace(".bank", "")}/");

		PushToConsoleLog($"\nExtracting Sound Files from {bankfilename}...\n", YELLOW);
		PushToConsoleLog($"Sounds Found: {bank.Samples.Count}", YELLOW);

		foreach (var bankSample in bank.Samples)
		{
			var name = bankSample.Name ?? $"UnknownSound-{Guid.NewGuid().ToString()[..8]}";
			if (!bankSample.RebuildAsStandardFileFormat(out byte[]? data, out string? extension))
			{
				PushToConsoleLog($"ERROR: Failed to Extract Sound {name}", RED);
				continue;
			}

			var filePath = Path.Combine(outDir.FullName, $"{name}.{extension}");
			File.WriteAllBytes(filePath, data);
			PushToConsoleLog($"Extracted Sound {name}.{extension}", CYAN);

			// set defaults for frequency and channels
			int frequency = bankSample.Metadata?.Frequency ?? 44100;
			uint numChannels = bankSample.Metadata?.Channels ?? 2;

			// add to xml
			AudioFile.AudioFileXML(outPath, filePath, frequency, numChannels);
		}
	}
}