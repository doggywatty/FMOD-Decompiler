using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using NAudio.Wave;
using NVorbis;
# region Get Set
public class objects
{
    [XmlAttribute("serializationModel")]
    public string SerializationModel { get; set; }

    [XmlElement("object")]
    public List<ObjectData> ObjectList { get; set; }
}

public class ObjectData
{
    [XmlAttribute("class")]
    public string Class { get; set; }

    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlElement("property")]
    public List<Property> Properties { get; set; }

    [XmlElement("relationship")]
    public List<Relationship> Relationships { get; set; }
}

public class Property
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("value")]
    public string Value { get; set; }
}

public class Relationship
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("destination")]
    public string Destination { get; set; }
}
#endregion

public class AudioFile
{
    public static void AudioFileXML(string outputpath, string soundfilepath, string bankfilename, int frequency, uint channels)
    {
        // TODO, get GUIDs, and finalize where to save
        string TEMP = "{00000000-0000-0000-0000-000000000000}";

        // Create the XML structure
        var xml = new objects
        {
            SerializationModel = "Studio.02.02.00",
            ObjectList = new List<ObjectData>
            {
                new ObjectData
                {
                    Class = "AudioFile",
                    // Id = It's own GUID
                    Id = $"{TEMP}",
                    Properties = new List<Property>
                    {
                        new Property { Name = "assetPath", Value = $"{Path.GetRelativePath(outputpath, soundfilepath)}" },
                        new Property { Name = "frequencyInKHz", Value = $"{frequency / 1000}" },
                        new Property { Name = "channelCount", Value = $"{channels}" },
                        new Property { Name = "length", Value = $"{GetAudioLength(soundfilepath)}" }
                    },
                    Relationships = new List<Relationship>
                    {
                        new Relationship { Name = "masterAssetFolder", Destination = $"{TEMP}" }// = Master GUID in /Asset folder
                    }
                }
            }
        };

        // XML File Path TEMP
        string filePath = outputpath.Replace("/Assets", "");// Go to root
        //                                                  temp solution until we get GUIDs
        filePath = filePath + "/Metadata/AudioFile/" + Path.GetFileNameWithoutExtension(soundfilepath) + ".xml";

        // Export XML
        var serializer = new XmlSerializer(typeof(objects));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", "");

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ", // 4 spaces indentation
            Encoding = new System.Text.UTF8Encoding(false)
        };
        // Serialize to XML and write to file using XmlWriter with UTF-8 encoding
        using (var writer = XmlWriter.Create(filePath, settings))
        {
            serializer.Serialize(writer, xml, namespaces);
        }
    }

    #region Find Sound Length
    // needed because FMOD5Sharp has no way of doing it, and we're not connected to the FMOD API/Bank here
    static float GetAudioLength(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();

        if (fileExtension == ".wav")
        {
            // Handle WAV file
            return GetWavDuration(filePath);
        }
        else if (fileExtension == ".ogg")
        {
            // Handle OGG file
            return GetOggDuration(filePath);
        }
        else
        {
            throw new NotSupportedException("Unsupported file format.");
        }
    }

    static float GetWavDuration(string filePath)
    {
        using (var reader = new WaveFileReader(filePath))
        {
            return (float)reader.TotalTime.TotalSeconds;
        }
    }
    static float GetOggDuration(string filePath)
    {
        using (var vorbis = new VorbisReader(filePath))
        {
            return (float)vorbis.TotalTime.TotalSeconds;
        }
    }
}
#endregion