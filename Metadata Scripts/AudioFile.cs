using System.Globalization;
using System.Xml;
using NAudio.Wave;
using NVorbis;
using static Program;
using static XMLHelper;

public class AudioFile
{
	public static void AudioFileXML(string outputpath, string soundfilepath, int frequency, uint channels)
	{
		var relativepath = Path.GetRelativePath(outputpath, soundfilepath);
		var SoundName = Path.GetFileNameWithoutExtension(relativepath);

		// ENSURE we have a GUID for this audio file (generate one if missing)
		if (!AudioFileGUIDs.TryGetValue(SoundName, out Guid audioGuid))
		{
			audioGuid = GetRandomGUID();
			AudioFileGUIDs[SoundName] = audioGuid;
			PushToConsoleLog($"WARNING: Generated missing AudioFile GUID for '{SoundName}'", YELLOW);
		}

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		// Add GUID of Current AudioFile XML
		SetupHeaderXML(xmlDoc, root, "AudioFile", $"{{{audioGuid}}}", out XmlElement objectElement);

		// Add AudioFile info
		AddPropertyElement(xmlDoc, objectElement, "assetPath", relativepath.Replace("\\", "/"));// because it was backwards
		AddPropertyElement(xmlDoc, objectElement, "frequencyInKHz", (frequency / 1000.0).ToString("0.0", CultureInfo.InvariantCulture));
		AddPropertyElement(xmlDoc, objectElement, "channelCount", channels.ToString(CultureInfo.InvariantCulture));
		AddPropertyElement(xmlDoc, objectElement, "length", GetAudioLength(soundfilepath).ToString("0.0####", CultureInfo.InvariantCulture));

		// Link back to MasterAssetFolder GUID
		AddRelationshipElement(xmlDoc, objectElement, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");

		xmlDoc.AppendChild(root);

		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/AudioFile/{{{audioGuid}}}.xml");
	}

	/*
		ProgrammerSound needs a placeholder AudioFile.
		The placeholder audio is stored as a loose file (never in a bank) and it can't be extracted,
		so generate a short silent WAV so the reference resolves and the project opens.
	*/
	public static Guid CreatePlaceholderAudioFile(string assetFolder)
	{
		Guid audioGuid = GetRandomGUID();
		string relFolder = assetFolder.Trim('/');
		string dir = $"{outputProjectPath}/Assets/{relFolder}";
		Directory.CreateDirectory(dir);

		string fileName = $"placeholder_{audioGuid.ToString()[..6]}_silent.wav";
		string filePath = Path.Combine(dir, fileName);

		const int sampleRate = 44100;
		const short channels = 2;
		const int seconds = 1;
		int numSamples = sampleRate * seconds;
		int dataBytes = numSamples * channels * 2;
		using (var ms = new MemoryStream())
		{
			using (var w = new BinaryWriter(ms))
			{
				w.Write(new[] { 'R', 'I', 'F', 'F' });
				w.Write(36 + dataBytes);
				w.Write(new[] { 'W', 'A', 'V', 'E' });
				w.Write(new[] { 'f', 'm', 't', ' ' });
				w.Write(16); // fmt chunk size
				w.Write((short)1); // PCM
				w.Write(channels);
				w.Write(sampleRate);
				w.Write(sampleRate * channels * 2); // byte rate
				w.Write((short)(channels * 2)); // block align
				w.Write((short)16); // bits per sample
				w.Write(new[] { 'd', 'a', 't', 'a' });
				w.Write(dataBytes);
				w.Write(new byte[dataBytes]);
			}
			File.WriteAllBytes(filePath, ms.ToArray());
		}

		string relativePath = Path.GetRelativePath($"{outputProjectPath}/Assets", filePath).Replace("\\", "/");

		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		SetupHeaderXML(xmlDoc, root, "AudioFile", $"{{{audioGuid}}}", out XmlElement objectElement);
		AddPropertyElement(xmlDoc, objectElement, "assetPath", relativePath);
		AddPropertyElement(xmlDoc, objectElement, "frequencyInKHz", "44.1");
		AddPropertyElement(xmlDoc, objectElement, "channelCount", "2");
		AddPropertyElement(xmlDoc, objectElement, "length", seconds.ToString(CultureInfo.InvariantCulture));
		AddRelationshipElement(xmlDoc, objectElement, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");
		xmlDoc.AppendChild(root);

		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/AudioFile/{{{audioGuid}}}.xml");
		return audioGuid;
	}

	private static float GetAudioLength(string filePath)
	{
		string ext = Path.GetExtension(filePath);
		if (string.Equals(ext, ".wav", StringComparison.OrdinalIgnoreCase))
		{
			using var reader = new WaveFileReader(filePath);
			return (float)reader.TotalTime.TotalSeconds;
		}
		if (string.Equals(ext, ".ogg", StringComparison.OrdinalIgnoreCase))
		{
			using var reader = new VorbisReader(filePath);
			return (float)reader.TotalTime.TotalSeconds;
		}
		return 0;
	}
}