using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Snapshots
{
	private static Dictionary<Guid, int> AllSnapshotsByPriority = [];

	public static void SnapshotXML(EventNode Evt, SnapshotNode? Snap, string SnapPath)
	{
		Guid AutomatablePropertiesGUID = GetRandomGUID();
		Guid MarkerTrackGUID = GetRandomGUID();
		Guid SnapshotMasterTrackGUID = GetRandomGUID();

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		/*
			The Snapshot object GUID is the Event GUID.
			SnapshotNode (Snap) holds behavior data linked via Evt.SnapshotGuid.
		*/
		Guid snapGuid = Evt.BaseGuid.ToGuid();
		int priority = Snap?.Priority ?? 0;
		lock (AllSnapshotsByPriority)
			AllSnapshotsByPriority[snapGuid] = priority;

		SetupHeaderXML(xmlDoc, root, "Snapshot", $"{{{Evt.BaseGuid}}}", out XmlElement SnapElement);

		string SnapName = SnapPath.Split('/')[^1];
		if (string.IsNullOrEmpty(SnapName))
			SnapName = $"Snapshot-{Evt.BaseGuid.ToString()[..8]}";
		AddPropertyElement(xmlDoc, SnapElement, "name", SnapName);
		bool blending = Snap?.BlendingSnapshot ?? false;
		AddPropertyElement(xmlDoc, SnapElement, "behavior", $"{(blending ? 1 : 0)}");

		AddRelationshipElement(xmlDoc, SnapElement, "mixer", $"{{{MasterMixerXMLGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "automatableProperties", $"{{{AutomatablePropertiesGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "markerTracks", $"{{{MarkerTrackGUID}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "timeline", $"{{{Evt.TimelineGuid}}}");
		AddRelationshipElement(xmlDoc, SnapElement, "snapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}");

		List<Guid> snapshotPropertyGuids = [];
		List<Guid> snapshotTrackGuids = [];

		if (Snap?.Snapshots != null)
		{
			foreach (var entry in Snap.Snapshots)
			{
				if (entry.SnapshotGuid.IsEmpty) continue;
				if ((int)entry.TargetIndex >= 1000) continue; // effect parameter, not resolvable
				if (!AllBusNodes.ContainsKey(entry.SnapshotGuid.ToString())) continue;

				Guid trackGuid = GetRandomGUID();
				snapshotTrackGuids.Add(trackGuid);
				SetupHeaderXML(xmlDoc, root, "SnapshotTrack", $"{{{trackGuid}}}", out XmlElement TrackElement);
				AddRelationshipElement(xmlDoc, TrackElement, "mixerStrip", $"{{{entry.SnapshotGuid}}}");

				Guid propGuid = GetRandomGUID();
				snapshotPropertyGuids.Add(propGuid);
				SetupHeaderXML(xmlDoc, root, "SnapshotProperty", $"{{{propGuid}}}", out XmlElement PropElement);
				AddPropertyElement(xmlDoc, PropElement, "propertyName", GetSnapshotPropertyName((int)entry.TargetIndex));
				AddPropertyElement(xmlDoc, PropElement, "value", $"{entry.Value}");
				AddRelationshipElement(xmlDoc, PropElement, "automatableObject", $"{{{entry.SnapshotGuid}}}");
			}
		}

		if (snapshotPropertyGuids.Count > 0)
			AddMultiRelationshipElement(xmlDoc, SnapElement, "snapshotProperties", [.. snapshotPropertyGuids]);
		if (snapshotTrackGuids.Count > 0)
			AddMultiRelationshipElement(xmlDoc, SnapElement, "snapshotTracks", [.. snapshotTrackGuids]);

		SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{AutomatablePropertiesGUID}}}", out XmlElement AutoPropElement);
		if (Snap != null && Snap.Intensity != 100)
			AddPropertyElement(xmlDoc, AutoPropElement, "snapshotIntensity", $"{Snap.Intensity}");

		SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGUID}}}", out _);
		SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{Evt.TimelineGuid}}}", out _);
		SetupHeaderXML(xmlDoc, root, "SnapshotMasterTrack", $"{{{SnapshotMasterTrackGUID}}}", out _);

		xmlDoc.AppendChild(root);

		// Save, snapshots live in Metadata/Snapshot (list lives in SnapshotGroup)
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Snapshot/{{{Evt.BaseGuid}}}.xml");
	}

	// Mixer-strip automatable property names, indexed by the bank's target index.
	private static string GetSnapshotPropertyName(int targetIndex) => targetIndex switch
	{
		0 => "volume",
		1 => "pitch",
		2 => "pan",
		3 => "lowpass",
		4 => "highpass",
		5 => "sendLevel",
		11 => "startOffset",
		_ => "volume",
	};

	// Legacy fallback, called only if a SnapshotNode has no matching Event (should be rare though)
	public static void SnapshotXML_Legacy(SnapshotNode Snap)
	{
		Guid AutomatablePropertiesGUID = GetRandomGUID();
		Guid MarkerTrackGUID = GetRandomGUID();
		Guid TimelineGUID = GetRandomGUID();
		Guid SnapshotMasterTrackGUID = GetRandomGUID();

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

		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Snapshot/{{{Snap.BaseGuid}}}.xml");
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