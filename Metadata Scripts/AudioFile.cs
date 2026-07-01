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