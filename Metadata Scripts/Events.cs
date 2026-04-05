using FModBankParser;
using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;
#pragma warning disable CS8602
public class Events
{
	public static void EventXML(EventNode Event, string EventPath, FModReader ParentBank)
	{
        #region Init Main GUIDs
        // these change per XML, but not within the XML
        // so they should be here, and not public static
        Guid EventMixerGuid = GetRandomGUID();
		Guid MasterTrackGuid = GetRandomGUID();
		Guid MixerInputGuid = GetRandomGUID();
		Guid EventAutomatablePropertiesGuid = GetRandomGUID();
		Guid MarkerTrackGuid = GetRandomGUID();
		Guid EventMixerMasterGuid = GetRandomGUID();
		Guid MixerBusEffectChainGuid1 = GetRandomGUID();
		Guid MixerBusEffectChainGuid2 = GetRandomGUID();
		Guid MixerBusPannerGuid1 = GetRandomGUID();
		Guid MixerBusPannerGuid2 = GetRandomGUID();
		Guid MixerBusFaderGuid1 = GetRandomGUID();
		Guid MixerBusFaderGuid2 = GetRandomGUID();

		Guid ActionSheetGuid = GetRandomGUID();
		#endregion

		// get the event's timeline (where all the sound and markers are stored)
		TimelineNode eTimeline = ParentBank.TimelineNodes[Event.TimelineGuid];

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		#region Main Event Info Links
		SetupHeaderXML(xmlDoc, root, "Event", $"{{{Event.BaseGuid}}}", out XmlElement EventElement);
		//											   get shortened event name out of event path
		AddPropertyElement(xmlDoc, EventElement, "name", GetName(EventPath));
		AddPropertyElement(xmlDoc, EventElement, "outputFormat", "0");
		//														  get name of folder containing the event
		AddRelationshipElement(xmlDoc, EventElement, "folder", $"{{{GetHigherEventFolder(EventPath)}}}");
		AddRelationshipElement(xmlDoc, EventElement, "mixer", $"{{{EventMixerGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "masterTrack", $"{{{MasterTrackGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "mixerInput", $"{{{MixerInputGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "automatableProperties", $"{{{EventAutomatablePropertiesGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "markerTracks", $"{{{MarkerTrackGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
		// Add Action Sheet if it is one
		//if (SoundsPresent && IsAction)
		//	AddRelationshipElement(xmlDoc, EventElement, "parameters", $"{{{ActionSheetGuid}}}");
		//														  connects event to its original bank file
		AddRelationshipElement(xmlDoc, EventElement, "banks", $"{{{ParentBank.BankInfo.BaseGuid}}}");
		#endregion

		#region Event Info
		SetupHeaderXML(xmlDoc, root, "EventMixer", $"{{{EventMixerGuid}}}", out XmlElement EventMixerElement);
		AddRelationshipElement(xmlDoc, EventMixerElement, "masterBus", $"{{{EventMixerMasterGuid}}}");

		#region Master Track
		SetupHeaderXML(xmlDoc, root, "MasterTrack", $"{{{MasterTrackGuid}}}", out XmlElement MasterTrackElement);
		/*
		// if audiofile on timeline (and isn't Action)
		if (SoundsPresent && !IsAction)
			AddMultiRelationshipElement(xmlDoc, MasterTrackElement, "modules", multisoundGUIDs);
		// else if sounds and is Action
		else if (SoundsPresent && IsAction)
			AddRelationshipElement(xmlDoc, MasterTrackElement, "modules", $"{{{MultiSoundGuid}}}");
		*/
		#endregion

		AddRelationshipElement(xmlDoc, MasterTrackElement, "mixerGroup", $"{{{EventMixerMasterGuid}}}");

		SetupHeaderXML(xmlDoc, root, "MixerInput", $"{{{MixerInputGuid}}}", out XmlElement MixerInputElement);
		AddRelationshipElement(xmlDoc, MixerInputElement, "effectChain", $"{{{MixerBusEffectChainGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "panner", $"{{{MixerBusPannerGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "output", $"{{{MasterXMLGUID}}}"); // connected to Master.xml

		#region EventAutomatableProperties (Macros)
		SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{EventAutomatablePropertiesGuid}}}", out XmlElement EventAutomatablePropertiesElement);
		// Don't add properties if they're at default value (or null)
		if (Event.Priority != 2) // 2 is Medium
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "priority", $"{Event.Priority}");
        if (Event.DopplerScale != 100) 
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "dopplerScale", $"{Event.DopplerScale}");
        if (Event.MinimumDistance != null && Event.MinimumDistance != 1) 
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "minimumDistance", $"{Event.MinimumDistance}");
        if (Event.MaximumDistance != null && Event.MaximumDistance != 20) 
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "maximumDistance", $"{Event.MaximumDistance}");
        if (Event.TriggerCooldown != null && Event.TriggerCooldown != 0)
            AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "triggerCooldown", $"{Event.TriggerCooldown}");
		#endregion

		// Master Marker i guess
		SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGuid}}}", out XmlElement MarkerTrackElement);

		#region Timeline Header (empty if no sounds or markers)
		SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{eTimeline.BaseGuid}}}", out XmlElement TimelineElement);
		/*
		// if there are sounds, but timeline
		if (SoundsPresent && !IsAction)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "modules", multisoundGUIDs);
		// else if there are sounds, and action sheet
		else if (SoundsPresent && IsAction)
			AddPropertyElement(xmlDoc, TimelineElement, "isProxyEnabled", "false");

		// Add Markers + Parameter Regions plus optional Sound Loop Regions to Timeline
		List<Guid> TimelineMarkerGuids = [];
		if (MarkersPresent)
			TimelineMarkerGuids.AddRange(MarkerGUIDs);
		if (ParametersPresent)
			TimelineMarkerGuids.AddRange(ParameterGUIDs);
		if (UseSoundLoopRegions)
			TimelineMarkerGuids.AddRange(SoundLoopRegionGUIDs);
		if (TimelineMarkerGuids.Count > 0)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "markers", [.. TimelineMarkerGuids]);
		*/
		#endregion
		#region Event Parameters (might have to revisit)
		if (Event.ParameterLayouts.Length > 0) 
		{
            foreach (var p in Event.ParameterLayouts)
            {
				ParameterLayoutNode pNode = ParentBank.ParameterLayoutNodes[p]; // get real node from bank
                SetupHeaderXML(xmlDoc, root, "ParameterProxy", $"{{{pNode.BaseGuid}}}", out XmlElement ParameterElement);
                AddRelationshipElement(xmlDoc, ParameterElement, "preset", $"{{{pNode.ParameterGuid}}}");
            }
        }
		#endregion

		// Main Action Sheet Header
		/*
		if (SoundsPresent && IsAction) 
		{
			SetupHeaderXML(xmlDoc, root, "ActionSheet", $"{{{ActionSheetGuid}}}", out XmlElement ActionSheetElement);
			AddRelationshipElement(xmlDoc, ActionSheetElement, "modules", $"{{{MultiSoundGuid}}}");
		}
		*/

		SetupHeaderXML(xmlDoc, root, "EventMixerMaster", $"{{{EventMixerMasterGuid}}}", out XmlElement EventMixerMasterElement);
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "effectChain", $"{{{MixerBusEffectChainGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "panner", $"{{{MixerBusPannerGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "mixer", $"{{{EventMixerGuid}}}");

		// Action Sheet Header for Sounds
		/*
		if (SoundsPresent && IsAction)
		{
			SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{MultiSoundGuid}}}", out XmlElement MultiSoundElement);
			AddMultiRelationshipElement(xmlDoc, MultiSoundElement, "sounds", multisoundGUIDs);
		}
		*/

		#region Single Sound Modules
		if (eTimeline.TriggerBoxes.Length > 0)
		{
			foreach (var s in eTimeline.TriggerBoxes)
			{
				SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{s.Guid}}}", out XmlElement SoundElement);
				if (s.StartTime != 0) 
					AddPropertyElement(xmlDoc, SoundElement, "start", $"{s.StartTime}");
				AddPropertyElement(xmlDoc, SoundElement, "length", $"{s.Length}");
				//AddPropertyElement(xmlDoc, SoundElement, "looping", $"true");// probably can't be done

				// TODO - link audiofile GUID
				// this might be an issue...
				//AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{sound.GUID}}}");
			}
		}
		#endregion

		SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", out XmlElement MixerBusEffectChainElement1);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement1, "effects", $"{{{MixerBusFaderGuid1}}}");

		// Empty for now
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}", out XmlElement MixerBusPannerElement1);

		#region Sustain Points
		if (eTimeline.SustainPoints.Length > 0)
        {
            foreach (var s in eTimeline.SustainPoints)
            {
                // TODO - GetRandomGUID is TEMP, since it needs to be added to Markers element (in Timeline element)
                SetupHeaderXML(xmlDoc, root, "SustainPoint", $"{{{GetRandomGUID()}}}", out XmlElement SustainPointElement);
                AddPropertyElement(xmlDoc, SustainPointElement, "position", $"{s.Position}");
                AddRelationshipElement(xmlDoc, SustainPointElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
                AddRelationshipElement(xmlDoc, SustainPointElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
				// TODO - maybe do something with Evaluators?
            }
        }
        #endregion
        #region Markers/Regions
        if (eTimeline.TimelineNamedMarkers.Length > 0)
        {
            foreach (var m in eTimeline.TimelineNamedMarkers)
            {
				// TODO - add to markers element
				string XMLHeader = (m.Length > 0) ? "LoopRegion" : "NamedMarker"; // Either Region or normal marker
                SetupHeaderXML(xmlDoc, root, XMLHeader, $"{{{m.BaseGuid}}}", out XmlElement NamedMarkerElement);
                AddPropertyElement(xmlDoc, NamedMarkerElement, "position", $"{m.Position}");

				// Region Only
                if (m.Length > 0) 
					AddPropertyElement(xmlDoc, NamedMarkerElement, "length", $"{m.Length}");

                AddPropertyElement(xmlDoc, NamedMarkerElement, "name", $"{m.Name}");

                // Normal = 0, Loop = 1, Magnet = 2
				// 1 is default
				// TODO - we can set it to Loop or Magnet if we can find out their type
                if (m.Length > 0)
                    AddPropertyElement(xmlDoc, NamedMarkerElement, "looping", "0");

                AddRelationshipElement(xmlDoc, NamedMarkerElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
                AddRelationshipElement(xmlDoc, NamedMarkerElement, "markerTrack", $"{{{MarkerTrackGuid}}}");

                // Parameter Conditions (for Magnet if we ever figure this out)
                //AddRelationshipElement(xmlDoc, NamedMarkerElement, "triggerConditions", $"{{{ParameterConditionGUIDs[i]}}}");
            }
        }
        #endregion
        #region Tempo Markers
        if (eTimeline.TimelineTempoMarkers.Length > 0)
        {
            foreach (var m in eTimeline.TimelineTempoMarkers)
            {
				// TODO - add to markers element
                SetupHeaderXML(xmlDoc, root, "TempoMarker", $"{{{m.BaseGuid}}}", out XmlElement TempoMarkerElement);
                AddPropertyElement(xmlDoc, TempoMarkerElement, "position", $"{m.Position}");
                AddPropertyElement(xmlDoc, TempoMarkerElement, "tempo", $"{m.Tempo}");
                AddPropertyElement(xmlDoc, TempoMarkerElement, "timeSignatureNumerator", $"{m.TimeSignature}");
                AddRelationshipElement(xmlDoc, TempoMarkerElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
                AddRelationshipElement(xmlDoc, TempoMarkerElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
            }
        }
        #endregion

        SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid2}}}", out XmlElement MixerBusEffectChainElement2);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement2, "effects", $"{{{MixerBusFaderGuid2}}}");

		// Empty Headers (for now maybe)
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid2}}}", out XmlElement MixerBusPannerElement2);
		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid1}}}", out XmlElement MixerBusFaderElement1);

		#region Index Parameter Conditions for Loop Regions
		/*
		if (ParametersPresent)
		{
			var i = 0;
			foreach (var param in ParametersInfo)
			{
				SetupHeaderXML(xmlDoc, root, "ParameterCondition", $"{{{ParameterConditionGUIDs[i]}}}", out XmlElement ParameterElement);
				AddPropertyElement(xmlDoc, ParameterElement, "minimum", $"{param.value}");
				AddPropertyElement(xmlDoc, ParameterElement, "maximum", $"{param.value}");

				// Link to actual parameter XML
				AddRelationshipElement(xmlDoc, ParameterElement, "parameter", $"{{{param.GUID}}}");
				i++;
			}
		}
		*/
		#endregion

		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid2}}}", out XmlElement MixerBusFaderElement2);
		#endregion

		// Save the XML document to a file
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Event/{{{Event.BaseGuid}}}.xml");
	}

	#region Get Event Names
	// Get Folder above
	public static string GetHigherEventFolder(string EventPath)
	{
        List<string> folders = SplitEventPath(EventPath);
        return (folders.Count >= 1) 
			? $"{EventFolderGUIDs[folders[^1] + $"{folders.Count - 1}"]}" // if like event:/music/soundtest/pause, or event:/soundtest/pause, get /soundtest
            : $"{MasterEventFolderGUID}"; // else if like event:/sound, get Master Folder

    }

	// Get Shortened Name
	public static string GetName(string EventPath)
	{
        // Get the last part (event name)
        List<string> folders = [.. EventPath.Split('/')];
		return $"{folders[^1]}";
	}
	#endregion
}
