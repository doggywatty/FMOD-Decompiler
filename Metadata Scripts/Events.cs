// Extract Event Info and Create Event XML
// EXTREMELY OBVIOUSLY WIP

// We definitely can't get everything
// so this is basically just a basic template to get events to appear in FMOD Studio
// anything important like sounds being there is not happening, due to how limited the FMOD API is

using System.Xml;
using static Program;
public class Events
{
    // TEMP
    public static string EventMixerGuid = "{50e04aed-2784-4866-bab6-e4d443a3e218}";
    public static string MasterTrackGuid = "{16d874fc-49a8-43bf-bb9c-98f7b6464cf4}";
    public static string MixerInputGuid = "{26a35029-0600-4147-bc3e-7feda46b67a1}";
    public static string EventAutomatablePropertiesGuid = "{81c3f48e-114a-4758-9083-24b68d8fe2dd}";
    public static string MarkerTrackGuid = "{d9f58a21-e880-418c-9b1d-bd3409fc4002}";
    public static string TimelineGuid = "{f6d244fb-9ed2-49f9-8f15-fd7cee804bd9}";
    public static string EventMixerMasterGuid = "{1f8caf30-35e4-4b0c-9022-b58634abe27f}";
    public static string MixerBusEffectChainGuid1 = "{43bdbbfa-9a96-473f-8ede-4d6380922b05}";
    public static string MixerBusEffectChainGuid2 = "{ee6e6524-5971-47a0-9bce-4aff1f095bd4}";
    public static string MixerBusPannerGuid1 = "{b87b13a5-e766-4a67-9994-0c8122308fe1}";
    public static string MixerBusPannerGuid2 = "{1ed73dbb-24d8-4975-b610-a3353104e07b}";
    public static string MixerBusFaderGuid1 = "{b2ce16cf-3af3-4a0a-9a2c-5e80f6215b03}";
    public static string MixerBusFaderGuid2 = "{d5b65c86-157f-43bd-b551-ebb692255f0a}";

    public static void SaveEvents(FMOD.Studio.EventInstance eventInstance, FMOD.Studio.EventDescription eventDescription, string eventname, string outputProjectPath)
    {
        // starter code
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declaration);
        XmlElement root = xmlDoc.CreateElement("objects");
        root.SetAttribute("serializationModel", "Studio.02.02.00");
        xmlDoc.AppendChild(root);

        // Add the other elements
        root.AppendChild(CreateObjectElement(xmlDoc, "Event", $"{{{EventGUIDs[eventname]}}}", new string[] { "name", "outputFormat" },
            new string[] { $"{GetName(eventname)}", "0" }, new (string, string)[]
            {
            ("folder", $"{{{GetHigherEventFolder(eventname)}}}"),
            // past here is UNFINISHED
            ("mixer", EventMixerGuid),
            ("masterTrack", MasterTrackGuid),
            ("mixerInput", MixerInputGuid),
            ("automatableProperties", EventAutomatablePropertiesGuid),
            ("markerTracks", MarkerTrackGuid),
            ("timeline", TimelineGuid)
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "EventMixer", EventMixerGuid, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("masterBus", EventMixerMasterGuid)
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MasterTrack", MasterTrackGuid, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("mixerGroup", EventMixerMasterGuid)
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MixerInput", MixerInputGuid, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effectChain", MixerBusEffectChainGuid1),
            ("panner", MixerBusPannerGuid1),
            ("output", "{04412813-250a-4a08-ac12-d4c0ee3a4ec0}")
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "EventAutomatableProperties", EventAutomatablePropertiesGuid));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MarkerTrack", MarkerTrackGuid));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "Timeline", TimelineGuid));

        root.AppendChild(CreateObjectElement(xmlDoc, "EventMixerMaster", EventMixerMasterGuid, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effectChain", MixerBusEffectChainGuid2),
            ("panner", MixerBusPannerGuid2),
            ("mixer", EventMixerGuid)
            })
        );

        root.AppendChild(CreateObjectElement(xmlDoc, "MixerBusEffectChain", MixerBusEffectChainGuid1, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effects", "{b2ce16cf-3af3-4a0a-9a2c-5e80f6215b03}")
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusPanner", MixerBusPannerGuid1));
        root.AppendChild(CreateObjectElement(xmlDoc, "MixerBusEffectChain", MixerBusEffectChainGuid2, new string[] { },
            new string[] { }, new (string, string)[]
            {
            ("effects", "{d5b65c86-157f-43bd-b551-ebb692255f0a}")
            })
        );

        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusPanner", MixerBusPannerGuid2));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusFader", MixerBusFaderGuid1));
        root.AppendChild(CreateEmptyObjectElement(xmlDoc, "MixerBusFader", MixerBusFaderGuid2));

        // Save the XML document to a file
        xmlDoc.Save(outputProjectPath + $"/Metadata/Event/{{{EventGUIDs[eventname]}}}.xml");
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

        // event:/music/soundtest/pause
        // after that
        if (folders.Count >= 2)
            return $"{EventFolderGUIDs[folders[folders.Count - 1]]}";
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
