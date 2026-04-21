using FModBankParser;
using FModBankParser.Nodes;
using FModBankParser.Nodes.ModulatorSubnodes;
using FModBankParser.Objects;
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

        #region Index Timeline Resources
        List<Guid> SoundModules = [];
        if (eTimeline.TriggerBoxes.Length > 0)
			foreach (var s in eTimeline.TriggerBoxes)
				SoundModules.Add(s.Guid.ToGuid());

        Dictionary<FSustainPoint, Guid> SustainPoints = [];
        if (eTimeline.SustainPoints.Length > 0)
			foreach (var s in eTimeline.SustainPoints)
				SustainPoints.Add(s, GetRandomGUID());

		List<Guid> Markers = [];
        if (eTimeline.TimelineNamedMarkers.Length > 0)
            foreach (var m in eTimeline.TimelineNamedMarkers)
                Markers.Add(m.BaseGuid.ToGuid());

		List<Guid> TempoMarkers = [];
        if (eTimeline.TimelineTempoMarkers.Length > 0)
            foreach (var m in eTimeline.TimelineTempoMarkers)
                TempoMarkers.Add(m.BaseGuid.ToGuid());
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
		// if audiofile on timeline (and isn't Action)
		if (SoundModules.Count > 0)// && !IsAction)
			AddMultiRelationshipElement(xmlDoc, MasterTrackElement, "modules", [.. SoundModules]);
		// else if sounds and is Action
		//else if (SoundsPresent && IsAction)
		//	AddRelationshipElement(xmlDoc, MasterTrackElement, "modules", $"{{{MultiSoundGuid}}}");
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
		// if there are sounds, but timeline
		if (SoundModules.Count > 0) //&& !IsAction)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "modules", [.. SoundModules]);
		// else if there are sounds, and action sheet
		//else if (SoundsPresent && IsAction)
		//	AddPropertyElement(xmlDoc, TimelineElement, "isProxyEnabled", "false");

		// Add references to elements onto Timeline
		List<Guid> TimelineMarkerGuids = [];
		if (SustainPoints.Count > 0)
			TimelineMarkerGuids.AddRange(SustainPoints.Values);
		if (Markers.Count > 0)
			TimelineMarkerGuids.AddRange(Markers);
		if (TempoMarkers.Count > 0)
			TimelineMarkerGuids.AddRange(TempoMarkers);

		if (TimelineMarkerGuids.Count > 0)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "markers", [.. TimelineMarkerGuids]);
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
		// see if there's modulators connected to this event
		List<ModulatorNode> Modulators = [.. ParentBank.ModulatorNodes.Values.Where(m => m.OwnerGuid == Event.BaseGuid)];
		if (Modulators.Count > 0) // add references to modulators if there are
			AddMultiRelationshipElement(xmlDoc, EventMixerMasterElement, "modulators", [.. Modulators.Select(m => m.BaseGuid.ToGuid())]);
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

		#region TODO - i actually think this is CMDB/EVIB (Nested Event)
		if (eTimeline.TriggerBoxes.Length > 0)
		{
			foreach (var s in eTimeline.TriggerBoxes)
			{
				string XMLHeader = "CommandSound";

                SetupHeaderXML(xmlDoc, root, XMLHeader, $"{{{s.Guid}}}", out XmlElement SoundElement);
				if (s.StartTime != 0) 
					AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(s.StartTime)}");
				AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(s.Length)}");
			}
		}
        #endregion
        #region Single Sound Modules
        if (eTimeline.TimeLockedTriggerBoxes.Length > 0)
        {
            foreach (var s in eTimeline.TimeLockedTriggerBoxes)
            {
                SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{s.Guid}}}", out XmlElement SoundElement);
                if (s.StartTime != 0)
                    AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(s.StartTime)}");
                AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(s.Length)}");
                //AddPropertyElement(xmlDoc, SoundElement, "looping", $"true");// probably can't be done

                // link audiofile GUID
                // im pretty sure FTriggerBox Guid is the same as the WavEntry Guid, so this should work, but idk
                // this very well might not work
                if (WavGUIDs.TryGetValue(s.Guid, out Guid AudioGuid))
                    AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{AudioGuid}}}");
            }
        }
        #endregion

        SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", out XmlElement MixerBusEffectChainElement1);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement1, "effects", $"{{{MixerBusFaderGuid1}}}");

		// Empty for now
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}", out XmlElement MixerBusPannerElement1);

        #region Modulators
        if (Modulators.Count > 0)
        {
            foreach (ModulatorNode Mod in Modulators)
            {
                string ModPropType = (uint)Mod.PropertyType switch
                {
                    0x0 => "normal", // unknown
                    0x1 => "volume",
                    _ => "volume",
                };
                switch ((int)Mod.Type)
				{
					case 0: // ADSR
						{
							ADSRModulatorNode? SubNode = (ADSRModulatorNode?)Mod.Subnode;
							if (SubNode is null) break;
                            SetupHeaderXML(xmlDoc, root, "ADSRModulator", $"{{{Mod.BaseGuid}}}", out XmlElement ADSRModulatorElement);
							AddPropertyElement(xmlDoc, ADSRModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
                            AddPropertyElement(xmlDoc, ADSRModulatorElement, "initialValue", $"{SubNode.InitialValue}");
							if (SubNode.AttackTime != 1000)
								AddPropertyElement(xmlDoc, ADSRModulatorElement, "attackTime", $"{SubNode.AttackTime}");
							if (SubNode.PeakValue != 1)
								AddPropertyElement(xmlDoc, ADSRModulatorElement, "peakValue", $"{SubNode.PeakValue}");
                            if (SubNode.HoldTime != 1000)
                                AddPropertyElement(xmlDoc, ADSRModulatorElement, "holdTime", $"{SubNode.HoldTime}");
                            if (SubNode.DecayTime != 1000)
                                AddPropertyElement(xmlDoc, ADSRModulatorElement, "decayTime", $"{SubNode.DecayTime}");
                            if (SubNode.SustainValue != 1)
                                AddPropertyElement(xmlDoc, ADSRModulatorElement, "sustainValue", $"{SubNode.SustainValue}");
                            if (SubNode.ReleaseTime != 1000)
                                AddPropertyElement(xmlDoc, ADSRModulatorElement, "releaseTime", $"{SubNode.ReleaseTime}");
							if (SubNode.FinalValue != null)
								AddPropertyElement(xmlDoc, ADSRModulatorElement, "finalValue", $"{SubNode.FinalValue}");
                            break;
                        }
                    case 1: // Random
                        {
							// NOTE - This only works for newer FMOD Studio versions
							//			since Min and Max are no longer used
                            RandomModulatorNode? SubNode = (RandomModulatorNode?)Mod.Subnode;
                            if (SubNode is null) break;
                            SetupHeaderXML(xmlDoc, root, "RandomizerModulator", $"{{{Mod.BaseGuid}}}", out XmlElement RandomizerModulatorElement);
                            AddPropertyElement(xmlDoc, RandomizerModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
							if (SubNode.Amount != 0)
								AddPropertyElement(xmlDoc, RandomizerModulatorElement, "amount", $"{SubNode.Amount}");
                            break;
                        }
					// can't do Envelope (2) because yeah
					case 3: // LFO
						{
                            LFOModulatorNode? SubNode = (LFOModulatorNode?)Mod.Subnode;
                            if (SubNode is null) break;
                            SetupHeaderXML(xmlDoc, root, "LFOModulator", $"{{{Mod.BaseGuid}}}", out XmlElement LFOModulatorElement);
                            AddPropertyElement(xmlDoc, LFOModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
							if (SubNode.Shape != 0) // Sine
								AddPropertyElement(xmlDoc, LFOModulatorElement, "shape", $"{SubNode.Shape}");
							if (SubNode.Rate != 0.50)
								AddPropertyElement(xmlDoc, LFOModulatorElement, "rate", $"{SubNode.Rate}");
                            if (SubNode.Phase != 0)
                                AddPropertyElement(xmlDoc, LFOModulatorElement, "phase", $"{SubNode.Phase}");
                            AddPropertyElement(xmlDoc, LFOModulatorElement, "depth", $"{SubNode.Amount}"); // misnamed for some reason
                            AddPropertyElement(xmlDoc, LFOModulatorElement, "direction", $"{SubNode.Direction}");
                            break;
						}
                    // can't do Seek (4) YAYYYY fucking why
                    case 5: // SpectralSidechain
                        {
                            SpectralSidechainModulatorNode? SubNode = (SpectralSidechainModulatorNode?)Mod.Subnode;
                            if (SubNode is null) break;
                            SetupHeaderXML(xmlDoc, root, "SidechainModulator", $"{{{Mod.BaseGuid}}}", out XmlElement SidechainModulatorElement);
                            AddPropertyElement(xmlDoc, SidechainModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
                            if (SubNode.Mode == 0) // if RMS Mode
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "levelMode", "1"); // the values are swapped?????
                            if (SubNode.Amount != 0)
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "amount", $"{SubNode.Amount}");
                            if (SubNode.AttackTime != 100)
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "attackTime", $"{SubNode.AttackTime}");
                            if (SubNode.ReleaseTime != 200)
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "releaseTime", $"{SubNode.ReleaseTime}");
                            if (SubNode.ThresholdMinimum != -24)
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "minimumThreshold", $"{SubNode.ThresholdMinimum}");
                            if (SubNode.ThresholdMaximum != -6)
                                AddPropertyElement(xmlDoc, SidechainModulatorElement, "maximumThreshold", $"{SubNode.ThresholdMaximum}");

							// TODO - there's a Guid that this modulator has, but idk how to get it on my test project, sooooo
                            break;
                        }
                }
            }
        }
		#endregion
		#region Sustain Points
		if (eTimeline.SustainPoints.Length > 0)
        {
            foreach (var s in eTimeline.SustainPoints)
            {
                SetupHeaderXML(xmlDoc, root, "SustainPoint", $"{{{SustainPoints[s]}}}", out XmlElement SustainPointElement);
                AddPropertyElement(xmlDoc, SustainPointElement, "position", $"{GetValue(s.Position)}");
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
				string XMLHeader = (m.Length > 0) ? "LoopRegion" : "NamedMarker"; // Either Region or normal marker
                SetupHeaderXML(xmlDoc, root, XMLHeader, $"{{{m.BaseGuid}}}", out XmlElement NamedMarkerElement);
                AddPropertyElement(xmlDoc, NamedMarkerElement, "position", $"{GetValue(m.Position)}");

				// Region Only
                if (m.Length > 0) 
					AddPropertyElement(xmlDoc, NamedMarkerElement, "length", $"{GetValue(m.Length)}");

				if (m.Name != string.Empty)
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
                SetupHeaderXML(xmlDoc, root, "TempoMarker", $"{{{m.BaseGuid}}}", out XmlElement TempoMarkerElement);
                AddPropertyElement(xmlDoc, TempoMarkerElement, "position", $"{GetValue(m.Position)}");
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
