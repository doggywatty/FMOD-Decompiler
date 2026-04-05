using System.Xml;
using NAudio.Wave;
using NVorbis;
using static Program;
using static XMLHelper;

public class AudioFile
{
    // NOTE
    // GUIDs for these ones are only referenced by themselves, or in events
    // but i doubt we can extract much from events, so yeah
    // If we can extract what audio files are used in events tho, we can use AudioFileGUIDs
    public static void AudioFileXML(string outputpath, string soundfilepath, int frequency, uint channels)
    {
        // Save GUID for this File
        var relativepath = Path.GetRelativePath(outputpath, soundfilepath);
        if (!AudioFileGUIDs.ContainsKey(relativepath))
            AudioFileGUIDs.Add(relativepath, GetRandomGUID());
        else //just in case there's a duplicate, ignore
        {
            PushToConsoleLog($"WARNING: Sound file {relativepath} is a duplicate\nSkipping...", YELLOW);
            return;
        }

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        // Add GUID of Current AudioFile XML
        SetupHeaderXML(xmlDoc, root, "AudioFile", $"{{{AudioFileGUIDs[relativepath]}}}", out XmlElement objectElement);

        // Add AudioFile info
        AddPropertyElement(xmlDoc, objectElement, "assetPath", relativepath.Replace("\\", "/"));// because it was backwards
        AddPropertyElement(xmlDoc, objectElement, "frequencyInKHz", (frequency / 1000).ToString());
        AddPropertyElement(xmlDoc, objectElement, "channelCount", channels.ToString());
        AddPropertyElement(xmlDoc, objectElement, "length", GetAudioLength(soundfilepath).ToString());

        // Link back to MasterAssetFolder GUID
        AddRelationshipElement(xmlDoc, objectElement, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");

        xmlDoc.AppendChild(root);

        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/AudioFile/{{{AudioFileGUIDs[relativepath]}}}.xml");
    }

    // needed because FMOD5Sharp has no way of doing it
    private static float GetAudioLength(string filePath)
    {
        dynamic reader;
        switch (Path.GetExtension(filePath).ToLower())
        {
            case ".wav": reader = new WaveFileReader(filePath); break;
            case ".ogg": reader = new VorbisReader(filePath); break;
            default: return 0;
        }
        return (float)reader.TotalTime.TotalSeconds;
    }
}