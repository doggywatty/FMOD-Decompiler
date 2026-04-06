using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Snapshots
{
    public static void SnapshotXML(SnapshotNode Snap)
    {
        Guid AutomatablePropertiesGUID = Guid.NewGuid();
        Guid MarkerTrackGUID = Guid.NewGuid();
        Guid TimelineGUID = Guid.NewGuid();
        Guid SnapshotMasterTrackGUID = Guid.NewGuid();

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "Snapshot", $"{{{Snap.BaseGuid}}}", out XmlElement SnapElement);

        if (StringTable != null && StringTable.TryGetString(Snap.BaseGuid, out string SnapPath))
        {
            string SnapName = SnapPath.Split('/')[^1];
            AddPropertyElement(xmlDoc, SnapElement, "name", SnapName);
        }
        AddPropertyElement(xmlDoc, SnapElement, "behavior", $"{(Snap.BlendingSnapshot ? 1 : 0)}");

        AddRelationshipElement(xmlDoc, SnapElement, "mixer", $"{{{MasterMixerXMLGUID}}}");
        AddRelationshipElement(xmlDoc, SnapElement, "automatableProperties", $"{{{AutomatablePropertiesGUID}}}");
        AddRelationshipElement(xmlDoc, SnapElement, "markerTracks", $"{{{MarkerTrackGUID}}}");
        AddRelationshipElement(xmlDoc, SnapElement, "timeline", $"{{{TimelineGUID}}}");
        AddRelationshipElement(xmlDoc, SnapElement, "snapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}");

        SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{AutomatablePropertiesGUID}}}", out XmlElement AutoPropElement);
        AddPropertyElement(xmlDoc, AutoPropElement, "snapshotIntensity", $"{Snap.Intensity}");

        SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGUID}}}", out _);
        SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{TimelineGUID}}}", out _);
        SetupHeaderXML(xmlDoc, root, "SnapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}", out _);

        xmlDoc.AppendChild(root);

        // Save
        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Snapshot/{{{Snap.BaseGuid}}}.xml");
    }

    public static void SnapshotGroupXML(List<Guid> SnapList)
    {
        Guid XMLGUID = Guid.NewGuid();

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "SnapshotList", $"{{{XMLGUID}}}", out XmlElement SnapListElement);
        AddMultiRelationshipElement(xmlDoc, SnapListElement, "items", [.. SnapList]);
        AddRelationshipElement(xmlDoc, SnapListElement, "mixer", $"{{{MasterMixerXMLGUID}}}");

        xmlDoc.AppendChild(root);

        // Save
        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/SnapshotGroup/{{{XMLGUID}}}.xml");
    }
}