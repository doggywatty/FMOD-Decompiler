using System.Xml;
using NAudio.Wave;
using NVorbis;
using static Program;

public class AudioFile
{
    // NOTE
    // GUIDs for these ones are only referenced by themselves, or in events
    // but i doubt we can extract much from events, so yeah
    // If we can extract what audio files are used in events tho, we can use AudioFileGUIDs
    public static void AudioFileXML(string outputpath, string soundfilepath, string bankfilename, int frequency, uint channels)
    {
        // Save GUID for this File
        AudioFileGUIDs.Add($"{Path.GetRelativePath(outputpath, soundfilepath)}", GetRandomGUID());

        // Setup XML
        var xmlDoc = new XmlDocument();
        var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDeclaration);
        var root = xmlDoc.CreateElement("objects");
        root.SetAttribute("serializationModel", "Studio.02.02.00");

        // Add GUID of Current AuidoFile XML
        var objectElement = xmlDoc.CreateElement("object");
        objectElement.SetAttribute("class", "AudioFile");
        objectElement.SetAttribute("id", $"{{{AudioFileGUIDs[$"{Path.GetRelativePath(outputpath, soundfilepath)}"]}}}");
        root.AppendChild(objectElement);

        // Add <property> elements with nested <value> elements
        AddPropertyElement(xmlDoc, objectElement, "assetPath", Path.GetRelativePath(outputpath, soundfilepath).Replace("\\","/"));// because it was backwards
        AddPropertyElement(xmlDoc, objectElement, "frequencyInKHz", (frequency / 1000).ToString());
        AddPropertyElement(xmlDoc, objectElement, "channelCount", channels.ToString());
        AddPropertyElement(xmlDoc, objectElement, "length", GetAudioLength(soundfilepath).ToString());

        var relationshipElement = xmlDoc.CreateElement("relationship");
        relationshipElement.SetAttribute("name", "masterAssetFolder");
        var destinationElement = xmlDoc.CreateElement("destination");
        destinationElement.InnerText = $"{{{MasterAssetsGUID}}}";
        relationshipElement.AppendChild(destinationElement);
        objectElement.AppendChild(relationshipElement);
        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputpath.Replace("/Assets", "");// Go to root
        filePath = filePath + "/Metadata/AudioFile/" + $"{{{AudioFileGUIDs[$"{Path.GetRelativePath(outputpath, soundfilepath)}"]}}}" + ".xml";

        // Save
        xmlDoc.Save(filePath);
    }
    private static void AddPropertyElement(XmlDocument xmlDoc, XmlElement parent, string propertyName, string value)
    {
        var propertyElement = xmlDoc.CreateElement("property");
        propertyElement.SetAttribute("name", propertyName);

        var valueElement = xmlDoc.CreateElement("value");
        valueElement.InnerText = value;

        propertyElement.AppendChild(valueElement);
        parent.AppendChild(propertyElement);
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