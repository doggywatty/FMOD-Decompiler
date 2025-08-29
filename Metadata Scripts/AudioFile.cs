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
            PushToConsoleLog($"WARNING! - Duplicate Sound file found: " + relativepath + "\nSkipping...");
            return;
        }

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        // Add GUID of Current AudioFile XML
        SetupHeaderXML(xmlDoc, root, "AudioFile", $"{{{AudioFileGUIDs[relativepath]}}}", out XmlElement objectElement);

        // Add AudioFile info
        AddPropertyElement(xmlDoc, objectElement, "assetPath", relativepath.Replace("\\","/"));// because it was backwards
        AddPropertyElement(xmlDoc, objectElement, "frequencyInKHz", (frequency / 1000).ToString());
        AddPropertyElement(xmlDoc, objectElement, "channelCount", channels.ToString());
        AddPropertyElement(xmlDoc, objectElement, "length", GetAudioLength(soundfilepath).ToString());

        // Link back to MasterAssetFolder GUID
        AddRelationshipElement(xmlDoc, objectElement, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + "/Metadata/AudioFile/" + $"{{{AudioFileGUIDs[relativepath]}}}" + ".xml";

        // Save
        SaveXML(xmlDoc, filePath);
    }

    #region Find Sound Length
    // needed because FMOD5Sharp has no way of doing it, and we're not connected to the FMOD API/Bank here
    static float GetAudioLength(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();

        // Handle WAV file
        if (fileExtension == ".wav")
            return GetWavDuration(filePath);
        // Handle OGG file
        else if (fileExtension == ".ogg")
            return GetOggDuration(filePath);
        // Unsupported format     uh no
        else
            throw new NotSupportedException("Unsupported file format.");
    }

    static float GetWavDuration(string filePath)
    {
        using (var reader = new WaveFileReader(filePath))
            return (float)reader.TotalTime.TotalSeconds;
    }
    static float GetOggDuration(string filePath)
    {
        using (var vorbis = new VorbisReader(filePath))
            return (float)vorbis.TotalTime.TotalSeconds;
    }
}
#endregion