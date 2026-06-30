using FModBankParser;
using FModBankParser.Nodes;
using FModBankParser.Nodes.Instruments;
using FModBankParser.Nodes.ModulatorSubnodes;
using FModBankParser.Nodes.Transitions;
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
		Guid MultiSoundGuid = GetRandomGUID();
		#endregion

		// get the event's timeline (where all the sound and markers are stored)
		TimelineNode eTimeline = ParentBank.TimelineNodes[Event.TimelineGuid];

		#region Index Timeline Resources
		List<FModGuid> SoundModules = [];
		if (eTimeline.TimeLockedTriggerBoxes.Length > 0)
			foreach (var s in eTimeline.TimeLockedTriggerBoxes)
				SoundModules.Add(s.Guid);

		List<FModGuid> NestModules = [];
		if (eTimeline.TriggerBoxes.Length > 0)
			foreach (var n in eTimeline.TriggerBoxes)
				NestModules.Add(n.Guid);

		List<FModGuid> AllModules = [.. SoundModules, .. NestModules];
		bool hasAnyModules = AllModules.Count > 0;

		// Filter to only modules that can resolve to actual audio files
		// Check both instrument chain resolution AND direct WAV GUID mapping
		List<FModGuid> ValidSoundModules = AllModules
			.Where(guid => ResolveWavResourceGuids(guid).Any() || WavGUIDs.ContainsKey(guid))
			.ToList();

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

		List<Guid> ParameterProxyGuids = [];
		if (Event.ParameterLayouts.Length > 0)
			foreach (var p in Event.ParameterLayouts)
				if (ParentBank.ParameterLayoutNodes.TryGetValue(p, out ParameterLayoutNode? pn))
					ParameterProxyGuids.Add(pn.BaseGuid.ToGuid());
		#endregion

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		#region Main Event Info Links
		SetupHeaderXML(xmlDoc, root, "Event", $"{{{Event.BaseGuid}}}", out XmlElement EventElement);

														/*	get shortened event name out of event path
														  	 |
														  	\ / */
		AddPropertyElement(xmlDoc, EventElement, "name", GetName(EventPath));
		AddPropertyElement(xmlDoc, EventElement, "outputFormat", "0");

																/*	get name of folder containing the event
																	 |
																	\ / */
		AddRelationshipElement(xmlDoc, EventElement, "folder", $"{{{GetHigherEventFolder(EventPath)}}}");

		AddRelationshipElement(xmlDoc, EventElement, "mixer", $"{{{EventMixerGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "masterTrack", $"{{{MasterTrackGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "mixerInput", $"{{{MixerInputGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "automatableProperties", $"{{{EventAutomatablePropertiesGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "markerTracks", $"{{{MarkerTrackGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "timeline", $"{{{eTimeline.BaseGuid}}}");

		// Parameters relationship: ActionSheet (if any modules) + ParameterProxy GUIDs
		List<Guid> ParamDestinations = [];
		if (hasAnyModules)
			ParamDestinations.Add(ActionSheetGuid);
		ParamDestinations.AddRange(ParameterProxyGuids);
		if (ParamDestinations.Count > 0)
			AddMultiRelationshipElement(xmlDoc, EventElement, "parameters", [.. ParamDestinations]);

																/*	connects event to its original bank file
																	 |
																	\ / */
		AddRelationshipElement(xmlDoc, EventElement, "banks", $"{{{ParentBank.BankInfo.BaseGuid}}}");
		#endregion

		#region Event Info
		SetupHeaderXML(xmlDoc, root, "EventMixer", $"{{{EventMixerGuid}}}", out XmlElement EventMixerElement);
		AddRelationshipElement(xmlDoc, EventMixerElement, "masterBus", $"{{{EventMixerMasterGuid}}}");

		#region Master Track
		SetupHeaderXML(xmlDoc, root, "MasterTrack", $"{{{MasterTrackGuid}}}", out XmlElement MasterTrackElement);
		if (hasAnyModules)
			AddRelationshipElement(xmlDoc, MasterTrackElement, "modules", $"{{{MultiSoundGuid}}}");
		AddRelationshipElement(xmlDoc, MasterTrackElement, "mixerGroup", $"{{{EventMixerMasterGuid}}}");
		#endregion

		SetupHeaderXML(xmlDoc, root, "MixerInput", $"{{{MixerInputGuid}}}", out XmlElement MixerInputElement);
		AddRelationshipElement(xmlDoc, MixerInputElement, "effectChain", $"{{{MixerBusEffectChainGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "panner", $"{{{MixerBusPannerGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "output", $"{{{MasterXMLGUID}}}");

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
		AddPropertyElement(xmlDoc, TimelineElement, "isProxyEnabled", "false");

		// Add references to markers onto Timeline
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

		SetupHeaderXML(xmlDoc, root, "EventMixerMaster", $"{{{EventMixerMasterGuid}}}", out XmlElement EventMixerMasterElement);
		AddPropertyElement(xmlDoc, EventMixerMasterElement, "volume", "0");
		// see if there's modulators connected to this event
		List<ModulatorNode> Modulators = [.. ParentBank.ModulatorNodes.Values.Where(m => m.OwnerGuid == Event.BaseGuid)];
		if (Modulators.Count > 0) // add references to modulators if there are
			AddMultiRelationshipElement(xmlDoc, EventMixerMasterElement, "modulators", [.. Modulators.Select(m => m.BaseGuid.ToGuid())]);
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "effectChain", $"{{{MixerBusEffectChainGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "panner", $"{{{MixerBusPannerGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "mixer", $"{{{EventMixerGuid}}}");

		#region Action Sheet
		if (hasAnyModules)
		{
			SetupHeaderXML(xmlDoc, root, "ActionSheet", $"{{{ActionSheetGuid}}}", out XmlElement ActionSheetElement);
			AddRelationshipElement(xmlDoc, ActionSheetElement, "modules", $"{{{MultiSoundGuid}}}");
		}
		#endregion

		#region MultiSound Wrapper
		if (hasAnyModules)
		{
			SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{MultiSoundGuid}}}", out XmlElement MultiSoundElement);
			AddMultiRelationshipElement(xmlDoc, MultiSoundElement, "sounds", [.. AllModules.Select(g => g.ToGuid())]);
		}
		#endregion

		// Log warning if no valid audio modules could be resolved
		if (ValidSoundModules.Count == 0 && hasAnyModules)
			PushToConsoleLog($"Warning: Event has no resolvable audio: {EventPath} (generating basic structure)", YELLOW);

		#region Commands, Nested Events, and Sound Modules
		if (eTimeline.TriggerBoxes.Length > 0)
		{
			foreach (var s in eTimeline.TriggerBoxes)
			{
				if (AllInstrumentNodes.TryGetValue(s.Guid, out BaseInstrumentNode? Instrument))
				{
					switch (Instrument)
					{
						case EventInstrumentNode e: // Nested Event
							{
								SetupHeaderXML(xmlDoc, root, "EventSound", $"{{{s.Guid}}}", out XmlElement NestElement);
								if (s.StartTime != 0)
									AddPropertyElement(xmlDoc, NestElement, "start", $"{GetValue(s.StartTime)}");
								AddPropertyElement(xmlDoc, NestElement, "length", $"{GetValue(s.Length)}");
								AddRelationshipElement(xmlDoc, NestElement, "event", $"{{{e.EventGuid}}}");
							}
							break;
						case CommandInstrumentNode c: // Command
							{
								SetupHeaderXML(xmlDoc, root, "CommandSound", $"{{{s.Guid}}}", out XmlElement CommandElement);
								if (s.StartTime != 0)
									AddPropertyElement(xmlDoc, CommandElement, "start", $"{GetValue(s.StartTime)}");
								AddPropertyElement(xmlDoc, CommandElement, "length", $"{GetValue(s.Length)}");
								AddPropertyElement(xmlDoc, CommandElement, "commandType", $"{c.CommandType}");
								AddPropertyElement(xmlDoc, CommandElement, "targetValue", $"{c.Value}");
								AddRelationshipElement(xmlDoc, CommandElement, "commandTarget", $"{{{c.TargetGuid}}}");
							}
							break;
						case WaveformInstrumentNode:
						case MultiInstrumentNode:
						case ScattererInstrumentNode:
							if (ValidSoundModules.Contains(s.Guid))
								CreateSingleSound(xmlDoc, root, s.Guid, s.StartTime, s.Length, ParentBank, EventPath);
							else
							{
								// Create bare SingleSound with position/length but no audioFile
								SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{s.Guid}}}", out XmlElement SoundElement);
								if (s.StartTime != 0)
									AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(s.StartTime)}");
								AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(s.Length)}");
							}
							break;
						default:
							PushToConsoleLog($"WARNING: Unknown instrument type {Instrument.GetType().Name} for trigger box {s.Guid} in event {EventPath}", YELLOW);
							if (ValidSoundModules.Contains(s.Guid))
								CreateSingleSound(xmlDoc, root, s.Guid, s.StartTime, s.Length, ParentBank, EventPath);
							break;
					}
				}
				else if (ValidSoundModules.Contains(s.Guid))
					CreateSingleSound(xmlDoc, root, s.Guid, s.StartTime, s.Length, ParentBank, EventPath);
				else
				{
					// Create a bare SingleSound even without resolvable audio to maintain structure
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{s.Guid}}}", out XmlElement SoundElement);
					if (s.StartTime != 0)
						AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(s.StartTime)}");
					AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(s.Length)}");
				}
			}
		}
		#endregion
		#region Single Sound Modules
		if (eTimeline.TimeLockedTriggerBoxes.Length > 0)
		{
			foreach (var s in eTimeline.TimeLockedTriggerBoxes)
			{
				if (ValidSoundModules.Contains(s.Guid))
					CreateSingleSound(xmlDoc, root, s.Guid, s.StartTime, s.Length, ParentBank, EventPath);
				else
				{
					// Create a bare SingleSound even without resolvable audio to maintain structure
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{s.Guid}}}", out XmlElement SoundElement);
					if (s.StartTime != 0)
						AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(s.StartTime)}");
					AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(s.Length)}");
				}
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
							if (SubNode is null)
								break;
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
							if (SubNode is null)
								break;
							SetupHeaderXML(xmlDoc, root, "RandomizerModulator", $"{{{Mod.BaseGuid}}}", out XmlElement RandomizerModulatorElement);
							AddPropertyElement(xmlDoc, RandomizerModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
							if (SubNode.Amount != 0)
								AddPropertyElement(xmlDoc, RandomizerModulatorElement, "amount", $"{SubNode.Amount}");
							break;
						}
					case 2: // Envelope
						{
							EnvelopeModulatorNode? SubNode = (EnvelopeModulatorNode?)Mod.Subnode;
							if (SubNode is null) break;
							SetupHeaderXML(xmlDoc, root, "EnvelopeModulator", $"{{{Mod.BaseGuid}}}", out XmlElement EnvelopeModulatorElement);
							AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
							if (SubNode.Amount != null && SubNode.Amount != 0)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "amount", $"{SubNode.Amount}");
							if (SubNode.AttackTime != null)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "attackTime", $"{SubNode.AttackTime}");
							if (SubNode.ReleaseTime != null)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "releaseTime", $"{SubNode.ReleaseTime}");
							if (SubNode.ThresholdMinimum != 0)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "minimumThreshold", $"{SubNode.ThresholdMinimum}");
							if (SubNode.ThresholdMaximum != 0)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "maximumThreshold", $"{SubNode.ThresholdMaximum}");
							if (SubNode.UseRMS != null && SubNode.UseRMS == true)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "levelMode", "1");
							if (SubNode.Minimum != null)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "minimum", $"{SubNode.Minimum}");
							if (SubNode.Maximum != null)
								AddPropertyElement(xmlDoc, EnvelopeModulatorElement, "maximum", $"{SubNode.Maximum}");
							if (SubNode.EffectId != null)
								AddRelationshipElement(xmlDoc, EnvelopeModulatorElement, "sidechain", $"{{{SubNode.EffectId}}}");
							break;
						}
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
					case 4: // Seek
						{
							SeekModulatorNode? SubNode = (SeekModulatorNode?)Mod.Subnode;
							if (SubNode is null) break;
							SetupHeaderXML(xmlDoc, root, "SeekModulator", $"{{{Mod.BaseGuid}}}", out XmlElement SeekModulatorElement);
							AddPropertyElement(xmlDoc, SeekModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
							if (SubNode.SeekSpeedAscending != 0)
								AddPropertyElement(xmlDoc, SeekModulatorElement, "seekSpeedAscending", $"{SubNode.SeekSpeedAscending}");
							if (SubNode.SeekSpeedDescending != 0)
								AddPropertyElement(xmlDoc, SeekModulatorElement, "seekSpeedDescending", $"{SubNode.SeekSpeedDescending}");
							break;
						}
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

							// Add the sidechain input reference if available
							if (SubNode.ThresholdMapping != default)
								AddRelationshipElement(xmlDoc, SidechainModulatorElement, "sidechain", $"{{{SubNode.ThresholdMapping}}}");
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

				// handle Evaluators IF present
				if (s.Evaluators != null && s.Evaluators.Count > 0)
				{
					foreach (var evaluator in s.Evaluators)
					{
						Guid evaluatorGuid = GetRandomGUID();
						SetupHeaderXML(xmlDoc, root, "Evaluator", $"{{{evaluatorGuid}}}", out XmlElement EvaluatorElement);
						AddPropertyElement(xmlDoc, EvaluatorElement, "type", $"{(int)evaluator.Type}");
						AddRelationshipElement(xmlDoc, EvaluatorElement, "sustainPoint", $"{{{SustainPoints[s]}}}");

						// Evaluator Data can be VARIOUS types, add as a generic property for now
						if (evaluator.Data != null)
							AddPropertyElement(xmlDoc, EvaluatorElement, "data", $"{evaluator.Data}");
					}
				}
			}
		}
		#endregion
		#region Markers/Regions
		if (eTimeline.TimelineNamedMarkers.Length > 0)
		{
			foreach (var m in eTimeline.TimelineNamedMarkers)
			{
				var tKvp = ParentBank.TransitionNodes.FirstOrDefault(t => t.Value is TransitionRegionNode r && r.DestinationGuid == m.BaseGuid);
				TransitionRegionNode? TransRegion = tKvp.Value is TransitionRegionNode rNode ? rNode : null;

				string XMLHeader = (m.Length > 0) ? "LoopRegion" : "NamedMarker"; // Either Region or normal marker
				SetupHeaderXML(xmlDoc, root, XMLHeader, $"{{{m.BaseGuid}}}", out XmlElement NamedMarkerElement);
				AddPropertyElement(xmlDoc, NamedMarkerElement, "position", $"{GetValue(m.Position)}");

				// Region Only
				if (m.Length > 0) 
					AddPropertyElement(xmlDoc, NamedMarkerElement, "length", $"{GetValue(m.Length)}");

				if (m.Name != string.Empty)
					AddPropertyElement(xmlDoc, NamedMarkerElement, "name", $"{m.Name}");

				if (TransRegion != null)
				{
					// Normal = 0, Loop = 1, Magnet = 2
					int RegionType = TransRegion.Flags switch
					{
						0x00 or 0x01 => 0,
						0x02 => 1,
						0x10 or 0x14 => 2,
						_ => 0,
					};
					if (RegionType != 1)
						AddPropertyElement(xmlDoc, NamedMarkerElement, "looping", $"{RegionType}");

					// Handle Parameter Conditions / Evaluators for Magnet regions
					if (TransRegion.Evaluators != null && TransRegion.Evaluators.Count > 0)
					{
						List<Guid> conditionGuids = [];
						foreach (var evaluator in TransRegion.Evaluators)
						{
							Guid conditionGuid = GetRandomGUID();
							conditionGuids.Add(conditionGuid);

							SetupHeaderXML(xmlDoc, root, "ParameterCondition", $"{{{conditionGuid}}}", out XmlElement ConditionElement);
							AddPropertyElement(xmlDoc, ConditionElement, "type", $"{(int)evaluator.Type}");

							if (evaluator.Data != null)
								AddPropertyElement(xmlDoc, ConditionElement, "data", $"{evaluator.Data}");
						}

						if (conditionGuids.Count > 0)
							AddMultiRelationshipElement(xmlDoc, NamedMarkerElement, "triggerConditions", [.. conditionGuids]);
					}
				}
				else if (m.Length > 0)
					AddPropertyElement(xmlDoc, NamedMarkerElement, "looping", "0");

				AddRelationshipElement(xmlDoc, NamedMarkerElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
				AddRelationshipElement(xmlDoc, NamedMarkerElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
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

				// decode time signature, FMOD stores numerator and denominator as a packed value
				// the value is a 64-bit field where the upper "32 bits = numerator" and lower "32 bits = denominator"
				ulong timeSig = (ulong)(long)m.TimeSignature;
				int numerator = (int)(timeSig >> 32);
				int denominator = (int)(timeSig & 0xFFFFFFFF);
				AddPropertyElement(xmlDoc, TempoMarkerElement, "timeSignatureNumerator", $"{numerator}");
				AddPropertyElement(xmlDoc, TempoMarkerElement, "timeSignatureDenominator", $"{denominator}");

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

		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid2}}}", out XmlElement MixerBusFaderElement2);
		#endregion

		// Save the XML document to a file
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Event/{{{Event.BaseGuid}}}.xml");
	}

	#region SingleSound Helper
	private static void CreateSingleSound(XmlDocument xmlDoc, XmlElement root, FModGuid guid, long startTime, long length, FModReader bank, string eventPath)
	{
		SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement SoundElement);

		if (startTime != 0)
			AddPropertyElement(xmlDoc, SoundElement, "start", $"{GetValue(startTime)}");
		AddPropertyElement(xmlDoc, SoundElement, "length", $"{GetValue(length)}");

		List<FModGuid> wavResourceGuids = ResolveWavResourceGuids(guid);
		foreach (var wavResGuid in wavResourceGuids)
		{
			if (WavGUIDs.TryGetValue(wavResGuid, out Guid AudioGuid))
			{
				AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{AudioGuid}}}");
				break;
			}
		}

		if (!wavResourceGuids.Any() && WavGUIDs.TryGetValue(guid, out Guid DirectAudioGuid))
			AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{DirectAudioGuid}}}");
		else if (!wavResourceGuids.Any())
		{
			// Try direct WAV GUID lookup by comparing GUID values
			foreach (var kvp in WavGUIDs)
			{
				if (kvp.Key.ToString().Contains(guid.ToString()))
				{
					AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{kvp.Value}}}");
					break;
				}
			}
		}

		if (!wavResourceGuids.Any() && !WavGUIDs.ContainsKey(guid))
			PushToConsoleLog($"WARNING: Could not resolve audio file for trigger box {guid} in event {eventPath}", YELLOW);
	}
	#endregion

	#region Instrument -> Wav Resource Resolver
	// Trigger box GUIDs point to instruments, not wav entries.
	// Resolve the instrument chain to find the actual wav resource GUID(s).
	private static List<FModGuid> ResolveWavResourceGuids(FModGuid instrumentGuid, HashSet<string>? visited = null)
	{
		List<FModGuid> results = [];
		visited ??= [];

		string key = instrumentGuid.ToString();
		if (!visited.Add(key))
			return results; // prevent infinite recursion

		if (!AllInstrumentNodes.TryGetValue(instrumentGuid, out BaseInstrumentNode? instrument))
			return results;

		switch (instrument)
		{
			case WaveformInstrumentNode wav: results.Add(wav.WaveformResourceGuid); break;
			case MultiInstrumentNode multi:
				if (multi.PlaylistBody?.Entries != null)
					foreach (var entry in multi.PlaylistBody.Entries)
						results.AddRange(ResolveWavResourceGuids(entry.Guid, visited));
				break;
			case ScattererInstrumentNode scatter:
				if (scatter.PlaylistBody?.Entries != null)
					foreach (var entry in scatter.PlaylistBody.Entries)
						results.AddRange(ResolveWavResourceGuids(entry.Guid, visited));
				break;
		}
		return results;
	}
	#endregion

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