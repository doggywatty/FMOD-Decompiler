// Extract Event Info and Create Event XML

// We definitely can't get everything
// so this is basically just a basic template to get events to appear in FMOD Studio
// anything important like sounds being there is not happening, due to how limited the FMOD API is

using System.Xml;
using static Program;
public class Events
{
    public static void SaveEvents(string eventname, string outputProjectPath, string bankfilename)
    {
        // these change per XML, but not within the XML
        // so they should be here, and not public static

        // if they are all the same across all XML files
        // try cleaning the project
        Guid EventMixerGuid = GetRandomGUID();
        Guid MasterTrackGuid = GetRandomGUID();
        Guid MixerInputGuid = GetRandomGUID();
        Guid EventAutomatablePropertiesGuid = GetRandomGUID();
        Guid MarkerTrackGuid = GetRandomGUID();
        Guid TimelineGuid = GetRandomGUID();
        Guid EventMixerMasterGuid = GetRandomGUID();
        Guid MixerBusEffectChainGuid1 = GetRandomGUID();
        Guid MixerBusEffectChainGuid2 = GetRandomGUID();
        Guid MixerBusPannerGuid1 = GetRandomGUID();
        Guid MixerBusPannerGuid2 = GetRandomGUID();
        Guid MixerBusFaderGuid1 = GetRandomGUID();
        Guid MixerBusFaderGuid2 = GetRandomGUID();

        // starter code
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declaration);
        XmlElement root = xmlDoc.CreateElement("objects");
        root.SetAttribute("serializationModel", "Studio.02.02.00");
        xmlDoc.AppendChild(root);

        // Add the other elements
        root.AppendChild(CreateObjectElement(xmlDoc, "Event", $"{{{EventGUIDs[eventname]}}}", new string[] { "name", "outputFormat" },
            // get shortened event name out of event path
            new string[] { $"{GetName(eventname)}", "0" }, new (string, string)[]
            {
                // get name of folder containing the event
            ("folder", $"{{{(No_Org == false ? GetHigherEventFolder(eventname) : MasterEventFolderGUID)}}}"),
            ("mixer", $"{{{EventMixerGuid}}}"),
            ("masterTrack", $"{{{MasterTrackGuid}}}"),

            ("mixerInput", $"{{{MixerInputGuid}}}"),
            ("automatableProperties", $"{{{EventAutomatablePropertiesGuid}}}"),
            ("markerTracks", $"{{{MarkerTrackGuid}}}"),
            ("timeline", $"{{{TimelineGuid}}}"),
            // connects event to its original bank file
            ("banks", $"{{{BankSpecificGUIDs[bankfilename + "_Bank"]}}}")
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "EventMixer", $"{{{EventMixerGuid}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("masterBus", $"{{{EventMixerMasterGuid}}}")
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MasterTrack", $"{{{MasterTrackGuid}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("mixerGroup", $"{{{EventMixerMasterGuid}}}")
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MixerInput", $"{{{MixerInputGuid}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effectChain", $"{{{MixerBusEffectChainGuid1}}}"),
            ("panner", $"{{{MixerBusPannerGuid1}}}"),
            ("output", $"{{{MasterXMLGUID}}}")  // only one that is actually connected to something (Master.xml)
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "EventAutomatableProperties", $"{{{EventAutomatablePropertiesGuid}}}"));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MarkerTrack", $"{{{MarkerTrackGuid}}}"));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "Timeline", $"{{{TimelineGuid}}}"));

        root.AppendChild(CreateObjectElement(xmlDoc, "EventMixerMaster", $"{{{EventMixerMasterGuid}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effectChain", $"{{{MixerBusEffectChainGuid2}}}"),
            ("panner", $"{{{MixerBusPannerGuid2}}}"),
            ("mixer", $"{{{EventMixerGuid}}}")
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effects", $"{{{MixerBusFaderGuid1}}}")
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}"));
        root.AppendChild(CreateObjectElement(xmlDoc, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid2}}}", new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effects", $"{{{MixerBusFaderGuid2}}}")
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusPanner", $"{{{MixerBusPannerGuid2}}}"));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusFader", $"{{{MixerBusFaderGuid1}}}"));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusFader", $"{{{MixerBusFaderGuid2}}}"));

        // Make Indentation Settings
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",  // Set 4 spaces for indentation
            NewLineOnAttributes = false  // avoid new lines
        };

        // Save the XML document to a file
        using (XmlWriter writer = XmlWriter.Create(outputProjectPath + $"/Metadata/Event/{{{EventGUIDs[eventname]}}}.xml", settings))
        {
            xmlDoc.WriteTo(writer);
        }
    }

    #region Get Event Names
    // Get Folder above
    public static string GetHigherEventFolder(string eventname)
    {
        // Split the path by "/"
        var pathParts = eventname.Split('/');

        // Get the subfolders
        var folders = new List<string>(pathParts);
        folders.RemoveAt(0); // Remove "event:"
        folders.RemoveAt(folders.Count - 1); // Remove the last part (it's not a folder)

        // if like event:/music/soundtest/pause, or event:/soundtest/pause, get /soundtest
        if (folders.Count >= 1)
            return $"{EventFolderGUIDs[folders[folders.Count - 1] + $"{folders.Count - 1}"]}";
        // else if like event:/sound, get Master Folder
        else
            return $"{MasterEventFolderGUID}";
    }

    // Get Shortened Name
    public static string GetName(string eventname)
    {
        // Split the path by "/"
        var pathParts = eventname.Split('/');

        // Get the last part (event name)
        var folders = new List<string>(pathParts);
        return $"{folders[folders.Count - 1]}";
    }
    #endregion

    #region XML Element Stuff
    // create object elements with properties and relationships
    private static XmlElement CreateObjectElement(XmlDocument doc, string className, string objectId, string[] propertyNames, string[] propertyValues, (string, string)[] relationships)
    {
        XmlElement objElement = doc.CreateElement("object");
        objElement.SetAttribute("class", className);
        objElement.SetAttribute("id", objectId);

        // Add properties
        for (int i = 0; i < propertyNames.Length; i++)
        {
            XmlElement propertyElement = doc.CreateElement("property");
            propertyElement.SetAttribute("name", propertyNames[i]);
            XmlElement valueElement = doc.CreateElement("value");
            valueElement.InnerText = propertyValues[i];
            propertyElement.AppendChild(valueElement);
            objElement.AppendChild(propertyElement);
        }

        // Add relationships
        foreach (var relationship in relationships)
        {
            XmlElement relationshipElement = doc.CreateElement("relationship");
            relationshipElement.SetAttribute("name", relationship.Item1);
            XmlElement destinationElement = doc.CreateElement("destination");
            destinationElement.InnerText = relationship.Item2;
            relationshipElement.AppendChild(destinationElement);
            objElement.AppendChild(relationshipElement);
        }

        return objElement;
    }

    // Create empty elements without properties and relationships
    private static XmlElement CreateEmptyObjectElement(XmlDocument doc, string className, string objectId)
    {
        XmlElement objElement = doc.CreateElement("object");
        objElement.SetAttribute("class", className);
        objElement.SetAttribute("id", objectId);
        return objElement;
    }
    #endregion
}
