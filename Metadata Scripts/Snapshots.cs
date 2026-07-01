using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Snapshots
{
	private static Dictionary<Guid, int> AllSnapshotsByPriority = [];

	public static void SnapshotXML(SnapshotNode Snap)
	{
		Guid AutomatablePropertiesGUID = GetRandomGUID();
		Guid MarkerTrackGUID = GetRandomGUID();
		Guid TimelineGUID = GetRandomGUID();
		Guid SnapshotMasterTrackGUID = GetRandomGUID();

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		Guid snapGuid = Snap.BaseGuid.ToGuid();
		lock (AllSnapshotsByPriority)
			AllSnapshotsByPriority[snapGuid] = Snap.Priority;

		SetupHeaderXML(xmlDoc, root, "Snapshot", $"{{{Snap.BaseGuid}}}", out XmlElement SnapElement);

		if (StringTable != null && StringTable.TryGetString(Snap.BaseGuid, out string SnapPath) && !string.IsNullOrEmpty(SnapPath))
		{
			string SnapName = SnapPath.Split('/')[^1];
			AddPropertyElement(xmlDoc, SnapElement, "name", SnapName);
		}
		else
			AddPropertyElement(xmlDoc, SnapElement, "name", $"Snapshot-{Snap.BaseGuid.ToString()[..8]}");
		AddPropertyElement(xmlDoc, SnapElement, "behavior", $"{(Snap.BlendingSnapshot ? 1 : 0)}");

		AddRelationshipElement(xmlDoc, SnapElement, "mixer", $"{{{MasterMixerXMLGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "automatableProperties", $"{{{AutomatablePropertiesGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "markerTracks", $"{{{MarkerTrackGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "timeline", $"{{{TimelineGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "snapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}");

		SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{AutomatablePropertiesGUID}}}", out XmlElement AutoPropElement);
		if (Snap.Intensity != 100)
			AddPropertyElement(xmlDoc, AutoPropElement, "snapshotIntensity", $"{Snap.Intensity}");

		SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGUID}}}", out _);
		SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{TimelineGUID}}}", out _);
		SetupHeaderXML(xmlDoc, root, "SnapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}", out _);

		xmlDoc.AppendChild(root);

		// Save
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/SnapshotGroup/{{{Snap.BaseGuid}}}.xml");
	}

	public static void SnapshotGroupXML()
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "SnapshotList", $"{{{MasterSnapshotGUID}}}", out XmlElement SnapListElement);
		if (AllSnapshotsByPriority.Count > 0)
		{
			var orderedSnaps = AllSnapshotsByPriority.OrderBy(s => s.Value).Select(s => s.Key).ToArray();
			AddMultiRelationshipElement(xmlDoc, SnapListElement, "items", orderedSnaps);
		}
		AddRelationshipElement(xmlDoc, SnapListElement, "mixer", $"{{{MasterMixerXMLGUID}}}");

		xmlDoc.AppendChild(root);

		// Save
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/SnapshotGroup/{{{MasterSnapshotGUID}}}.xml");
	}
}