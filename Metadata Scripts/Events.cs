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
		Guid TimelineGuid = GetRandomGUID();
		Guid EventMixerMasterGuid = GetRandomGUID();
		Guid MixerBusEffectChainGuid1 = GetRandomGUID();
		Guid MixerBusEffectChainGuid2 = GetRandomGUID();
		Guid MixerBusPannerGuid1 = GetRandomGUID();
		Guid MixerBusPannerGuid2 = GetRandomGUID();
		Guid MixerBusFaderGuid1 = GetRandomGUID();
		Guid MixerBusFaderGuid2 = GetRandomGUID();

		Guid ActionSheetGuid = GetRandomGUID();
		#endregion

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
		AddRelationshipElement(xmlDoc, EventElement, "timeline", $"{{{TimelineGuid}}}");
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

		// Empty Header
		SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{EventAutomatablePropertiesGuid}}}", out XmlElement EventAutomatablePropertiesElement);

		// Master Marker i guess
		SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGuid}}}", out XmlElement MarkerTrackElement);

		#region Timeline Header (empty if no sounds or markers)
		SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{TimelineGuid}}}", out XmlElement TimelineElement);
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

		#region Index Single Sounds
		/*
		if (SoundsPresent)
		{
			var i = 0;
			foreach (var sound in SoundsinEvent)
			{
				SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{multisoundGUIDs[i]}}}", out XmlElement SoundElement);
				// where the sound starts on the timeline (in seconds)
				if (sound.startpos != 0)
					AddPropertyElement(xmlDoc, SoundElement, "start", $"{sound.startpos}");
				// length in milliseconds
				AddPropertyElement(xmlDoc, SoundElement, "length", $"{sound.length}");

				// TODO - Check if multiple sounds loop, and just make a Loop Region instead if they are
				if (SoundLoops.ContainsKey(sound.name) && SoundLoops[sound.name] > 0)
					AddPropertyElement(xmlDoc, SoundElement, "looping", $"true");

				// link audiofile GUID (always there)
				AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{sound.GUID}}}");
				i++;
			}
		}
		*/
		#endregion

		SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", out XmlElement MixerBusEffectChainElement1);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement1, "effects", $"{{{MixerBusFaderGuid1}}}");

		// Empty for now
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}", out XmlElement MixerBusPannerElement1);

		// Mainly used for Magnet Regions with Parameters, but could be used for normal Loop regions
		#region Index Loop Regions
		/*
		if (UseSoundLoopRegions)
		{
			var i = 0;
			foreach (var sound in LoopingSounds)
			{
				SetupHeaderXML(xmlDoc, root, "LoopRegion", $"{{{SoundLoopRegionGUIDs[i]}}}", out XmlElement LoopElement);
				AddPropertyElement(xmlDoc, LoopElement, "position", $"{sound.startpos}");
				AddPropertyElement(xmlDoc, LoopElement, "length", $"{sound.length}");
				// Normal = 0, Loop = 1, Magnet = 2
				AddPropertyElement(xmlDoc, LoopElement, "looping", "1");

				AddRelationshipElement(xmlDoc, LoopElement, "timeline", $"{{{TimelineGuid}}}");
				AddRelationshipElement(xmlDoc, LoopElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
				i++;
			}
		}
		if (ParametersPresent)
		{
			var i = 0;
			foreach (var param in ParametersInfo)
			{
				SetupHeaderXML(xmlDoc, root, "LoopRegion", $"{{{ParameterGUIDs[i]}}}", out XmlElement LoopElement);
				AddPropertyElement(xmlDoc, LoopElement, "position", $"{param.start}");
				AddPropertyElement(xmlDoc, LoopElement, "length", $"{param.length}");
				// Normal = 0, Loop = 1 (missing), Magnet = 2
				AddPropertyElement(xmlDoc, LoopElement, "looping", "2");

				AddRelationshipElement(xmlDoc, LoopElement, "timeline", $"{{{TimelineGuid}}}");
				AddRelationshipElement(xmlDoc, LoopElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
				// Parameter Conditions (not needed for a Loop Region)
				AddRelationshipElement(xmlDoc, LoopElement, "triggerConditions", $"{{{ParameterConditionGUIDs[i]}}}");
				i++;
			}
		}
		*/
		#endregion

		#region Index Markers
		/*
		if (MarkersPresent)
		{
			var i = 0;
			foreach (var marker in MarkersInfo)
			{
				SetupHeaderXML(xmlDoc, root, "NamedMarker", $"{{{MarkerGUIDs[i]}}}", out XmlElement MarkerElement);
				AddPropertyElement(xmlDoc, MarkerElement, "position", $"{marker.position}");
				AddPropertyElement(xmlDoc, MarkerElement, "name", $"{marker.name}");
				AddRelationshipElement(xmlDoc, MarkerElement, "timeline", $"{{{TimelineGuid}}}");
				AddRelationshipElement(xmlDoc, MarkerElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
				i++;
			}
		}
		*/
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
