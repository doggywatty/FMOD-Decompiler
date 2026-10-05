using Fmod5Sharp.FmodTypes;
using static Program;

public class ExtractSoundAssets
{
	// originally from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
	
	// Audio-table banks (no events/instruments) keep their files loose next to the project instead of under /Assets, so isAudioTable routes them to <project>/<dir>/<LANG>/.
	public static void ExtractSoundFiles(FmodSoundBank bank, string bankfilename, bool isAudioTable = false, string? audioTableDir = null)
	{
		string outPath = outputProjectPath + "/Assets";
		string targetDir = outPath;

		if (isAudioTable)
		{
			// e.g. Dialogue_CN -> <project>/Dialogue/CN
			string baseDir = audioTableDir ?? bankfilename;
			string lang = bankfilename.Length > baseDir.Length ? bankfilename[baseDir.Length..].TrimStart('_') : "";
			targetDir = lang.Length > 0 ? $"{outputProjectPath}/{baseDir}/{lang}" : $"{outputProjectPath}/{baseDir}";
		}

		var outDir = Directory.CreateDirectory(isAudioTable ? targetDir : $"{outPath}/{bankfilename}/");
		PushToConsoleLog($"\nExtracting Sound Files from {bankfilename}...", YELLOW);
		PushToConsoleLog($"Sounds Found: {bank.Samples.Count}", YELLOW);

		foreach (var bankSample in bank.Samples)
		{
			var name = bankSample.Name ?? $"UnknownSound-{Guid.NewGuid().ToString()[..8]}";
			// Sample names can already carry an extension; strip it to avoid "name.ogg.ogg".
			string baseName = Path.GetFileNameWithoutExtension(name);
			if (string.IsNullOrEmpty(baseName))
				baseName = $"UnknownSound-{Guid.NewGuid().ToString()[..8]}";

			if (!bankSample.RebuildAsStandardFileFormat(out byte[]? data, out string? extension))
			{
				PushToConsoleLog($"ERROR: Failed to Extract Sound {name}", RED);
				continue;
			}

			var filePath = Path.Combine(outDir.FullName, $"{baseName}.{extension}");
			File.WriteAllBytes(filePath, data);
			PushToConsoleLog($"Extracted Sound {baseName}.{extension}", CYAN);

			// set defaults for frequency and channels
			int frequency = bankSample.Metadata?.Frequency ?? 44100;
			uint numChannels = bankSample.Metadata?.Channels ?? 2;

			// add to xml, audio tables aren't AudioFile objects, so skip them
			if (!isAudioTable)
				AudioFile.AudioFileXML(outPath, filePath, frequency, numChannels);
		}
	}
}
