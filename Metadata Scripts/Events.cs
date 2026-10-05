using FModBankParser;
using FModBankParser.Enums;
using FModBankParser.Nodes;
using FModBankParser.Nodes.Instruments;
using FModBankParser.Nodes.ModulatorSubnodes;
using FModBankParser.Nodes.Transitions;
using FModBankParser.Objects;
using System.Xml;
using static Program;
using static XMLHelper;
public class Events
{
	/*
		Modulators already defined for the event currently being written.
		An instrument can be reached through both a timeline trigger box and a playlist entry, so each modulator object must only be emitted once per event.
	*/
	private static HashSet<string> emittedModulators = new();

	// Asset folder used when generating ProgrammerSound placeholders, mirrors the event's folder path (e.g. "Character/Radio") so it sits next to its siblings.
	private static string EventsPlaceholderFolder = "";

	public static void EventXML(EventNode Event, string EventPath, FModReader ParentBank)
	{
		EventsPlaceholderFolder = GetFolderPath(EventPath);
		emittedModulators = new();
		#region Init GUIDs - preserve real bank GUIDs where available
		Guid EventMixerGuid = GetRandomGUID();
		Guid MasterTrackGuid = GetRandomGUID(); // MasterTrack has no bank GUID, generated
		// MixerInput IS the InputBus (preserve real GUID when present)
		Guid MixerInputGuid;
		bool hasRealMixerInput = !Event.InputBusGuid.IsEmpty;
		if (hasRealMixerInput)
			MixerInputGuid = Event.InputBusGuid.ToGuid();
		else
			MixerInputGuid = GetRandomGUID();
		Guid EventAutomatablePropertiesGuid = GetRandomGUID();
		Guid MarkerTrackGuid = GetRandomGUID();
		// EventMixerMaster IS the MasterBus (preserve real MasterTrackGuid from bank)
		Guid EventMixerMasterGuid;
		bool hasRealMasterBus = !Event.MasterTrackGuid.IsEmpty;
		if (hasRealMasterBus)
			EventMixerMasterGuid = Event.MasterTrackGuid.ToGuid();
		else
			EventMixerMasterGuid = GetRandomGUID();
		Guid MixerBusEffectChainGuid1 = GetRandomGUID();
		Guid MixerBusEffectChainGuid2 = GetRandomGUID();
		Guid MixerBusPannerGuid1 = GetRandomGUID();
		Guid MixerBusPannerGuid2 = GetRandomGUID();
		Guid MixerBusFaderGuid1 = GetRandomGUID();
		Guid MixerBusFaderGuid2 = GetRandomGUID();
		#endregion

		if (!ParentBank.TimelineNodes.TryGetValue(Event.TimelineGuid, out TimelineNode? eTimeline) || eTimeline is null)
		{
			PushToConsoleLog($"WARNING: Timeline {Event.TimelineGuid} not found for event {EventPath}, skipping", YELLOW);
			return;
		}

		#region Index Timeline and Parameter instruments
		// All instruments placed on parameter sheets for this event
		HashSet<string> paramInstrumentSet = new();
		Dictionary<string, ParameterLayoutNode> layoutByGuid = new();
		if (Event.ParameterLayouts.Length > 0)
		{
			foreach (var p in Event.ParameterLayouts)
			{
				if (ParentBank.ParameterLayoutNodes.TryGetValue(p, out ParameterLayoutNode? pNode) && pNode is not null)
				{
					layoutByGuid[p.ToString()] = pNode;
					if (pNode.Instruments != null)
						foreach (var ig in pNode.Instruments)
							paramInstrumentSet.Add(ig.ToString());
				}
			}
		}

		/*
			Timeline trigger boxes (preserve bank order, TimeLocked first like before, then TriggerBoxes)
			Command instruments are dropped, their XML schema is not recoverable and a guessed schema makes FMOD Studio reject the whole event
		*/
		List<(FModGuid guid, long start, long length)> timelineEntries = new();
		if (eTimeline.TimeLockedTriggerBoxes.Length > 0)
			foreach (var s in eTimeline.TimeLockedTriggerBoxes)
				if (!IsUnrecoverableInstrument(s.Guid))
					timelineEntries.Add((s.Guid, s.StartTime, s.Length));
		if (eTimeline.TriggerBoxes.Length > 0)
			foreach (var n in eTimeline.TriggerBoxes)
				if (!IsUnrecoverableInstrument(n.Guid))
					timelineEntries.Add((n.Guid, n.StartTime, n.Length));

		// Split timeline entries into owned parameters and actual timeline (for safety, normally disjoint)
		List<(FModGuid guid, long start, long length)> trueTimelineEntries = new();
		foreach (var te in timelineEntries)
		{
			if (!paramInstrumentSet.Contains(te.guid.ToString()))
				trueTimelineEntries.Add(te);
		}

		// Group timeline instruments by routable bus -> NonMasterTrack
		List<FModGuid> nmtList = (Event.NonMasterTracks ?? Array.Empty<FModGuid>()).ToList();
		Dictionary<string, List<(FModGuid guid, long start, long length)>> trackModules = new();
		foreach (var nmt in nmtList)
			trackModules[nmt.ToString()] = new();
		List<(FModGuid guid, long start, long length)> unassigned = new();
		foreach (var te in trueTimelineEntries)
		{
			string? routableKey = null;
			if (AllInstrumentNodes.TryGetValue(te.guid, out BaseInstrumentNode? inst) && inst?.InstrumentBody?.Routable.BaseGuid is FModGuid rb && !rb.IsEmpty)
				routableKey = rb.ToString();
			// Also check parent-bank instruments directly (same GUIDs, but AllInstrumentNodes is global)
			if (routableKey != null && trackModules.ContainsKey(routableKey))
				trackModules[routableKey].Add(te);
			else if (nmtList.Count > 0)
				unassigned.Add(te);
			else
				unassigned.Add(te);
		}
		// Distribute unassigned round-robin starting with first track (preserves Pistol grouping when routable matches)
		if (nmtList.Count > 0 && unassigned.Count > 0)
		{
			// If routable matching already placed everything, unassigned holds only unknown-routing ones -> put on first track
			string firstKey = nmtList[0].ToString();
			foreach (var u in unassigned)
				trackModules[firstKey].Add(u);
			unassigned.Clear();
		}

		// Parameter instruments also live on GroupTracks (the project that i used, the original Car Engine GroupTrack mirrors ParameterProxy).
		// Assign them by routable bus so parameter-only events still get populated tracks.
		HashSet<string> alreadyTracked = new(trueTimelineEntries.Select(t => t.guid.ToString()));
		foreach (var layoutKvp in layoutByGuid)
		{
			var pNode = layoutKvp.Value;
			if (pNode.Instruments is null) continue;
			foreach (var ig in pNode.Instruments)
			{
				if (IsUnrecoverableInstrument(ig))
					continue;
				string ikey = ig.ToString();
				if (!alreadyTracked.Add(ikey))
					continue; // already placed as timeline instrument
				string? rkey = null;
				if (AllInstrumentNodes.TryGetValue(ig, out BaseInstrumentNode? pinst) && pinst?.InstrumentBody?.Routable.BaseGuid is FModGuid prb && !prb.IsEmpty)
					rkey = prb.ToString();
				if (rkey != null && trackModules.ContainsKey(rkey))
					trackModules[rkey].Add((ig, 0, 0));
				else if (nmtList.Count > 0)
					trackModules[nmtList[0].ToString()].Add((ig, 0, 0));
			}
		}

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

		/*
			GroupTrack new GUIDs per NMT
			EventMixerGroup ID = NMT
		*/
		Dictionary<string, Guid> groupTrackGuidByNmt = new();
		Dictionary<string, (Guid ec, Guid panner, Guid fader)> mixerGroupPartsByNmt = new();
		foreach (var nmt in nmtList)
		{
			groupTrackGuidByNmt[nmt.ToString()] = GetRandomGUID();
			mixerGroupPartsByNmt[nmt.ToString()] = (GetRandomGUID(), GetRandomGUID(), GetRandomGUID());
		}
		#endregion

		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		#region Event Parameters - resolve first, emit after the Event object
		/*
			The Event object must be the FIRST object in the file (that is how fmod studio writes every project),
			so the parameter sheets are resolved up-front here and their objects are created further down
			emitting them before the Event made fmod studio load the file but silently drop the events contents
		*/
		List<(Guid proxyGuid, Guid parameterGuid, List<Guid> modules)> paramSheets = [];
		if (Event.ParameterLayouts.Length > 0)
		{
			foreach (var p in Event.ParameterLayouts)
			{
				if (!ParentBank.ParameterLayoutNodes.TryGetValue(p, out ParameterLayoutNode? pNode) || pNode is null)
				{
					PushToConsoleLog($"Warning: Parameter layout {p} not found in bank for event, skipping", YELLOW);
					continue;
				}

				/*
					ParameterProxy.preset points at the GameParameter itself, NOT at the ParameterPreset that wraps it.
					verified against the examples project, the Car Engine sheet links to "e25821f0", which is the GameParameter object inside the RPM ParameterPreset file
					pointing at the wrapper made fmod studio reject every event that uses a parameter
				*/
				string paramKey = pNode.ParameterGuid.ToString();
				if (!EmittedParameterPresets.Contains(paramKey))
				{
					PushToConsoleLog($"Warning: Parameter {pNode.ParameterGuid} was never emitted (event {EventPath}); parameter sheet left unlinked", YELLOW);
					continue;
				}

				// Instruments on this parameter sheet, minus any we cannot represent
				List<Guid> sheetModules = [];
				if (pNode.Instruments != null)
					foreach (var g in pNode.Instruments)
						if (!IsUnrecoverableInstrument(g))
							sheetModules.Add(g.ToGuid());

				paramSheets.Add((pNode.BaseGuid.ToGuid(), pNode.ParameterGuid.ToGuid(), sheetModules));
			}
		}
		#endregion

		#region Main Event Info Links
		SetupHeaderXML(xmlDoc, root, "Event", $"{{{Event.BaseGuid}}}", out XmlElement EventElement);
		AddPropertyElement(xmlDoc, EventElement, "name", GetName(EventPath));
		/*
			little note here: "outputFormat" is an authored mixer setting
			it is NOT stored in the bank, so it cannot be recovered, it is deliberately omitted rather than guessed, which lets fmod studio fallback to the project default instead of locking to a wrong speaker layout
		*/
		AddRelationshipElement(xmlDoc, EventElement, "folder", $"{{{GetHigherEventFolder(EventPath)}}}");
		AddRelationshipElement(xmlDoc, EventElement, "mixer", $"{{{EventMixerGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "masterTrack", $"{{{MasterTrackGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "mixerInput", $"{{{MixerInputGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "automatableProperties", $"{{{EventAutomatablePropertiesGuid}}}");
		AddRelationshipElement(xmlDoc, EventElement, "markerTracks", $"{{{MarkerTrackGuid}}}");
		if (groupTrackGuidByNmt.Count > 0)
			AddMultiRelationshipElement(xmlDoc, EventElement, "groupTracks", groupTrackGuidByNmt.Values.ToArray());
		AddRelationshipElement(xmlDoc, EventElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
		// "parameters" is written before "banks" to match fmod studio's own element order.
		if (paramSheets.Count > 0)
			AddMultiRelationshipElement(xmlDoc, EventElement, "parameters", [.. paramSheets.Select(s => s.proxyGuid)]);
		AddRelationshipElement(xmlDoc, EventElement, "banks", $"{{{ParentBank.BankInfo.BaseGuid}}}");
		#endregion

		#region Event Info
		SetupHeaderXML(xmlDoc, root, "EventMixer", $"{{{EventMixerGuid}}}", out XmlElement EventMixerElement);
		AddRelationshipElement(xmlDoc, EventMixerElement, "masterBus", $"{{{EventMixerMasterGuid}}}");

		SetupHeaderXML(xmlDoc, root, "MasterTrack", $"{{{MasterTrackGuid}}}", out XmlElement MasterTrackElement);
		AddRelationshipElement(xmlDoc, MasterTrackElement, "mixerGroup", $"{{{EventMixerMasterGuid}}}");

		SetupHeaderXML(xmlDoc, root, "MixerInput", $"{{{MixerInputGuid}}}", out XmlElement MixerInputElement);
		AddRelationshipElement(xmlDoc, MixerInputElement, "effectChain", $"{{{MixerBusEffectChainGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "panner", $"{{{MixerBusPannerGuid1}}}");
		AddRelationshipElement(xmlDoc, MixerInputElement, "output", $"{{{EventOutputBusGUID}}}");

		SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{EventAutomatablePropertiesGuid}}}", out XmlElement EventAutomatablePropertiesElement);
		/*
			priority, bank default
			128 = omit (Studio default)
			Observed: 64->3 (Low)
			192->1 (High)
		*/
		if (Event.Priority != 128)
		{
			int xmlPrio = Event.Priority switch
			{
				64 => 3,
				192 => 1,
				<= 32 => 0,
				<= 96 => 3,
				<= 160 => 2,
				_ => 1,
			};
			// Omit Medium (2) which is the default
			if (xmlPrio != 2)
				AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "priority", $"{xmlPrio}");
		}
		// persistent flag, bit 1 (value 2) in Event.Flags
		if (((Event.Flags ?? 0) & 2) != 0)
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "isPersistent", "true");
		/*
			doppler, bank 0 = default/omit
			100 = enabled (original uses dopplerEnabled).
		*/
		if (Event.DopplerScale.HasValue && Event.DopplerScale.Value != 0)
		{
			if (Math.Abs(Event.DopplerScale.Value - 100) < 0.001)
				AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "dopplerEnabled", "true");
			else
				AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "dopplerScale", $"{Event.DopplerScale.Value}");
		}
		if (Event.MinimumDistance != null && Event.MinimumDistance != 1)
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "minimumDistance", $"{Event.MinimumDistance}");
		if (Event.MaximumDistance != null && Event.MaximumDistance != 20)
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "maximumDistance", $"{Event.MaximumDistance}");
		if (Event.TriggerCooldown != null && Event.TriggerCooldown != 0)
			AddPropertyElement(xmlDoc, EventAutomatablePropertiesElement, "triggerCooldown", $"{Event.TriggerCooldown}");

		SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGuid}}}", out XmlElement MarkerTrackElement);

		#region Group Tracks + Mixer Groups (preserve real bus GUIDs)
		int groupTrackIndex = 0;
		foreach (var nmt in nmtList)
		{
			string key = nmt.ToString();
			Guid gTrack = groupTrackGuidByNmt[key];
			var parts = mixerGroupPartsByNmt[key];

			SetupHeaderXML(xmlDoc, root, "GroupTrack", $"{{{gTrack}}}", out XmlElement GroupTrackElement);
			if (trackModules.TryGetValue(key, out var mods) && mods.Count > 0)
				AddMultiRelationshipElement(xmlDoc, GroupTrackElement, "modules", mods.Select(m => m.guid.ToGuid()).ToArray());
			AddRelationshipElement(xmlDoc, GroupTrackElement, "mixerGroup", $"{{{nmt}}}");

			SetupHeaderXML(xmlDoc, root, "EventMixerGroup", $"{{{nmt}}}", out XmlElement MixerGroupElement);
			groupTrackIndex++;
			AddPropertyElement(xmlDoc, MixerGroupElement, "name", GetMixerGroupName(groupTrackIndex));
			AddRelationshipElement(xmlDoc, MixerGroupElement, "effectChain", $"{{{parts.ec}}}");
			AddRelationshipElement(xmlDoc, MixerGroupElement, "panner", $"{{{parts.panner}}}");
			AddRelationshipElement(xmlDoc, MixerGroupElement, "output", $"{{{EventMixerMasterGuid}}}");

			SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{parts.ec}}}", out XmlElement ecEl);
			AddRelationshipElement(xmlDoc, ecEl, "effects", $"{{{parts.fader}}}");
			SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{parts.panner}}}", out _);
			SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{parts.fader}}}", out _);
		}
		#endregion

		SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{eTimeline.BaseGuid}}}", out XmlElement TimelineElement);
		
		// "isProxyEnabled" is written by fmod studio only when the event actually has parameter
		if (paramSheets.Count > 0)
			AddPropertyElement(xmlDoc, TimelineElement, "isProxyEnabled", "false");

		List<Guid> TimelineMarkerGuids = [];
		if (SustainPoints.Count > 0)
			TimelineMarkerGuids.AddRange(SustainPoints.Values);
		if (Markers.Count > 0)
			TimelineMarkerGuids.AddRange(Markers);
		if (TempoMarkers.Count > 0)
			TimelineMarkerGuids.AddRange(TempoMarkers);

		if (TimelineMarkerGuids.Count > 0)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "markers", [.. TimelineMarkerGuids]);

		// Timeline.modules are all timeline instruments
		if (trueTimelineEntries.Count > 0)
			AddMultiRelationshipElement(xmlDoc, TimelineElement, "modules", trueTimelineEntries.Select(t => t.guid.ToGuid()).ToArray());
		#endregion

		#region Parameter sheet objects (after the Event object, as Studio writes them)
		foreach (var sheet in paramSheets)
		{
			SetupHeaderXML(xmlDoc, root, "ParameterProxy", $"{{{sheet.proxyGuid}}}", out XmlElement ParameterElement);
			if (sheet.modules.Count > 0)
				AddMultiRelationshipElement(xmlDoc, ParameterElement, "modules", [.. sheet.modules]);
			// "preset" links the sheet to the GameParameter object itself
			AddRelationshipElement(xmlDoc, ParameterElement, "preset", $"{{{sheet.parameterGuid}}}");
		}
		#endregion

		SetupHeaderXML(xmlDoc, root, "EventMixerMaster", $"{{{EventMixerMasterGuid}}}", out XmlElement EventMixerMasterElement);
		List<ModulatorNode> EventModulators = [.. ParentBank.ModulatorNodes.Values.Where(m => m.OwnerGuid.ToString() == Event.BaseGuid.ToString())];
		if (EventModulators.Count > 0)
			AddMultiRelationshipElement(xmlDoc, EventMixerMasterElement, "modulators", [.. EventModulators.Select(m => m.BaseGuid.ToGuid())]);
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "effectChain", $"{{{MixerBusEffectChainGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "panner", $"{{{MixerBusPannerGuid2}}}");
		AddRelationshipElement(xmlDoc, EventMixerMasterElement, "mixer", $"{{{EventMixerGuid}}}");

		// Unassigned timeline instruments when there are no GroupTracks (no NMT), put on MasterTrack
		if (nmtList.Count == 0 && trueTimelineEntries.Count > 0)
			AddMultiRelationshipElement(xmlDoc, MasterTrackElement, "modules", trueTimelineEntries.Select(t => t.guid.ToGuid()).ToArray());

		#region Timeline Instruments
		HashSet<string> emittedInstruments = new();
		foreach (var te in trueTimelineEntries)
			EmitInstrument(xmlDoc, root, te.guid, te.start, te.length, ParentBank, EventPath, emittedInstruments, isParameterChild: false);
		#endregion
		#region Parameter Instruments
		foreach (var p in Event.ParameterLayouts)
		{
			if (!ParentBank.ParameterLayoutNodes.TryGetValue(p, out ParameterLayoutNode? pNode) || pNode?.Instruments is null)
				continue;
			/*
				On a parameter sheet, "start/length" are PARAMETER VALUES, not times, the sound is active between them
				I verified against the examples Car Engine, whose RPM parameter has sounds with start=5000 length=1600.
			*/
			Dictionary<string, (float s, float e)> rangeByInstr = new();
			if (pNode.TriggerBoxes != null)
				foreach (var tb in pNode.TriggerBoxes)
					rangeByInstr[tb.InstrumentGuid.ToString()] = (tb.Start, tb.End);

			foreach (var ig in pNode.Instruments)
			{
				if (IsUnrecoverableInstrument(ig))
					continue;
				if (rangeByInstr.TryGetValue(ig.ToString(), out var range))
					EmitParameterSound(xmlDoc, root, ig, range.s, range.e, ParentBank, EventPath, emittedInstruments);
				else
					EmitInstrument(xmlDoc, root, ig, 0, 0, ParentBank, EventPath, emittedInstruments, isParameterChild: true);
			}
		}
		#endregion

		SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", out XmlElement MixerBusEffectChainElement1);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement1, "effects", $"{{{MixerBusFaderGuid1}}}");
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}", out XmlElement MixerBusPannerElement1);

		#region Modulators (event-level)
		if (EventModulators.Count > 0)
		{
			foreach (ModulatorNode Mod in EventModulators)
				EmitModulator(xmlDoc, root, Mod);
		}
		// Instrument modulators are emitted with their instruments (see EmitInstrument)
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
				EmitTriggerConditions(xmlDoc, root, SustainPointElement, s.Evaluators);
			}
		}
		#endregion
		#region Markers/Regions
		if (eTimeline.TimelineNamedMarkers.Length > 0)
		{
			foreach (var m in eTimeline.TimelineNamedMarkers)
			{
				string XMLHeader = (m.Length > 0) ? "LoopRegion" : "NamedMarker";
				SetupHeaderXML(xmlDoc, root, XMLHeader, $"{{{m.BaseGuid}}}", out XmlElement NamedMarkerElement);
				AddPropertyElement(xmlDoc, NamedMarkerElement, "position", $"{GetValue(m.Position)}");
				if (m.Length > 0)
					AddPropertyElement(xmlDoc, NamedMarkerElement, "length", $"{GetValue(m.Length)}");
				if (m.Name != string.Empty)
					AddPropertyElement(xmlDoc, NamedMarkerElement, "name", $"{m.Name}");
				if (m.Length > 0)
					AddPropertyElement(xmlDoc, NamedMarkerElement, "looping", "0");

				AddRelationshipElement(xmlDoc, NamedMarkerElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
				AddRelationshipElement(xmlDoc, NamedMarkerElement, "markerTrack", $"{{{MarkerTrackGuid}}}");
			}
		}

		/*
			Transition regions drive the loop/transition behavior of markers.
			Each region targets a marker (destination) and may be gated by parameter conditions.
			TransitionNodes are stored per-bank (not per-event), so only keep the regions whose destination is a marker on this event's timeline,
			otherwise every event in the bank would re-emit the same regions under duplicate GUIDs.
		*/
		HashSet<string> thisEventMarkers = new(eTimeline.TimelineNamedMarkers.Select(m => m.BaseGuid.ToString()));
		foreach (var tKvp in ParentBank.TransitionNodes)
		{
			if (tKvp.Value is not TransitionRegionNode TransRegion)
				continue;
			if (!thisEventMarkers.Contains(TransRegion.DestinationGuid.ToString()))
				continue;

			SetupHeaderXML(xmlDoc, root, "TransitionRegion", $"{{{TransRegion.BaseGuid}}}", out XmlElement TransElement);
			AddPropertyElement(xmlDoc, TransElement, "position", $"{GetValue(TransRegion.Start)}");
			uint span = TransRegion.End >= TransRegion.Start ? TransRegion.End - TransRegion.Start : 0;
			if (span > 0)
				AddPropertyElement(xmlDoc, TransElement, "length", $"{GetValue(span)}");
			if (TransRegion.Quantization.Unit != EQuantizationUnit.None)
			{
				// The bank stores a unit and bar multiplier, so FMOD Studio stores the resulting interval in beats.
				double beats = TransRegion.Quantization.Unit switch
				{
					EQuantizationUnit.Bar => 4.0 * TransRegion.Quantization.Multiplier,
					EQuantizationUnit.EighthNote => 0.5 * TransRegion.Quantization.Multiplier,
					_ => 0.0,
				};
				if (beats > 0)
					AddPropertyElement(xmlDoc, TransElement, "quantizationInterval", $"{beats}");
			}

			AddRelationshipElement(xmlDoc, TransElement, "timeline", $"{{{eTimeline.BaseGuid}}}");
			AddRelationshipElement(xmlDoc, TransElement, "markerTrack", $"{{{MarkerTrackGuid}}}");

			/*
				TransitionTimeline carries the transition length plus the destination/source sounds and fade curves (fade curves themselves are authoring-only and are not stored in the bank)
				"destination" belongs to the owning TransitionRegion, NOT to the TransitionTimeline, the fmod schema has no such relationship on it
			*/
			Guid transTimelineGuid = GetRandomGUID();
			SetupHeaderXML(xmlDoc, root, "TransitionTimeline", $"{{{transTimelineGuid}}}", out XmlElement TransTlElement);
			uint tlLen = TransRegion.TransitionBody?.Length ?? 0;
			AddPropertyElement(xmlDoc, TransTlElement, "length", $"{GetValue(tlLen)}");
			AddRelationshipElement(xmlDoc, TransElement, "transitionTimeline", $"{{{transTimelineGuid}}}");
			if (!TransRegion.DestinationGuid.IsEmpty)
				AddRelationshipElement(xmlDoc, TransElement, "destination", $"{{{TransRegion.DestinationGuid}}}");

			// Parameter conditions, Type11 carries the parameter GUID, Type20 carries [min, max].
			EmitTriggerConditions(xmlDoc, root, TransElement, TransRegion.Evaluators);
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
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid2}}}", out XmlElement MixerBusPannerElement2);
		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid1}}}", out XmlElement MixerBusFaderElement1);
		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid2}}}", out XmlElement MixerBusFaderElement2);

		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Event/{{{Event.BaseGuid}}}.xml");
	}

	private static void EmitInstrument(XmlDocument xmlDoc, XmlElement root, FModGuid guid, long startTime, long length, FModReader bank, string eventPath, HashSet<string> emitted, bool isParameterChild)
	{
		string key = guid.ToString();
		if (!emitted.Add(key))
			return;
		if (!AllInstrumentNodes.TryGetValue(guid, out BaseInstrumentNode? instrument))
		{
			// unknown instrument, preserve GUID as bare SingleSound so Timeline.modules still resolves
			SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement unknownEl);
			if (startTime != 0)
				AddPropertyElement(xmlDoc, unknownEl, "start", $"{GetValue(startTime)}");
			if (length != 0)
				AddPropertyElement(xmlDoc, unknownEl, "length", $"{GetValue(length)}");
			PushToConsoleLog($"WARNING: Unknown instrument {guid} in event {eventPath}", YELLOW);
			return;
		}

		// Instrument modulators
		List<ModulatorNode> instMods = [.. bank.ModulatorNodes.Values.Where(m => m.OwnerGuid.ToString() == guid.ToString())];
		// Fallback to global scan if bank-specific misses (instruments are global, modulators per-bank)
		if (instMods.Count == 0)
		{
			// AllInstrumentNodes is global but ModulatorNodes are per-bank, ParentBank is the correct bank here
		}

		switch (instrument)
		{
			case EventInstrumentNode e:
				{
					SetupHeaderXML(xmlDoc, root, "EventSound", $"{{{guid}}}", out XmlElement el);
					if (startTime != 0)
						AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
					if (length != 0)
						AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					AddRelationshipElement(xmlDoc, el, "event", $"{{{e.EventGuid}}}");
					break;
				}
			case CommandInstrumentNode:
				{
					/*
						Command sounds are deliberately NOT emitted
						Their XML schema does not appear in any reference project and guessing it produced events Studio rejected as invalid components.
						They are filtered out of the track module lists before emission, so nothing is left referencing a missing object.
					*/
					break;
				}
			case ProgrammerInstrumentNode:
				{
					SetupHeaderXML(xmlDoc, root, "ProgrammerSound", $"{{{guid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isAsync", "true");
					if (startTime != 0)
						AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
					if (length != 0)
						AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					// fmod studio requires a placeholder AudioFile on every programmer sound
					Guid placeholder = AudioFile.CreatePlaceholderAudioFile(EventsPlaceholderFolder);
					AddRelationshipElement(xmlDoc, el, "placeholder", $"{{{placeholder}}}");
					break;
				}
			case SilenceInstrumentNode s:
				{
					SetupHeaderXML(xmlDoc, root, "SilenceSound", $"{{{guid}}}", out XmlElement el);
					if (startTime != 0)
						AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
					// Duration from bank is authoritative when timeline length is 0
					double lenSec = length != 0 ? GetValue(length) : s.Duration;
					if (lenSec != 0)
						AddPropertyElement(xmlDoc, el, "length", $"{lenSec}");
					break;
				}
			case WaveformInstrumentNode:
				{
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement el);
					if (!isParameterChild)
					{
						if (startTime != 0)
							AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
						if (length != 0)
							AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					}
					AttachModulators(el, instMods);
					EmitModulatorObjects(xmlDoc, root, instMods);
					Guid? audio = ResolveAudioGuid(guid);
					if (audio.HasValue)
						AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{audio}}}");
					else
						PushToConsoleLog($"WARNING: Could not resolve audio file for sound {guid} in event {eventPath}", YELLOW);
					break;
				}
			case MultiInstrumentNode multi:
				{
					SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{guid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isAsync", "true");
					if (!isParameterChild)
					{
						if (startTime != 0)
							AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
						if (length != 0)
							AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					}
					AttachModulators(el, instMods);
					EmitModulatorObjects(xmlDoc, root, instMods);
					if (multi.PlaylistBody?.Entries != null && multi.PlaylistBody.Entries.Length > 0)
						AddMultiRelationshipElement(xmlDoc, el, "sounds", multi.PlaylistBody.Entries.Select(en => en.Guid.ToGuid()).ToArray());
					// Emit playlist children as SingleSound entries
					if (multi.PlaylistBody?.Entries != null)
					{
						foreach (var en in multi.PlaylistBody.Entries)
							EmitPlaylistChild(xmlDoc, root, en.Guid, bank, eventPath, emitted);
					}
					break;
				}
			case ScattererInstrumentNode scatter:
				{
					SetupHeaderXML(xmlDoc, root, "SoundScatterer", $"{{{guid}}}", out XmlElement el);
					if (!isParameterChild)
					{
						if (startTime != 0)
							AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
						if (length != 0)
							AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					}
					AttachModulators(el, instMods);
					EmitModulatorObjects(xmlDoc, root, instMods);
					if (scatter.PlaylistBody?.Entries != null && scatter.PlaylistBody.Entries.Length > 0)
						AddMultiRelationshipElement(xmlDoc, el, "sounds", scatter.PlaylistBody.Entries.Select(en => en.Guid.ToGuid()).ToArray());
					if (scatter.PlaylistBody?.Entries != null)
					{
						foreach (var en in scatter.PlaylistBody.Entries)
							EmitPlaylistChild(xmlDoc, root, en.Guid, bank, eventPath, emitted);
					}
					break;
				}
			default:
				{
					PushToConsoleLog($"WARNING: Unknown instrument type {instrument.GetType().Name} for {guid} in event {eventPath}", YELLOW);
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement el);
					if (startTime != 0)
						AddPropertyElement(xmlDoc, el, "start", $"{GetValue(startTime)}");
					if (length != 0)
						AddPropertyElement(xmlDoc, el, "length", $"{GetValue(length)}");
					Guid? audio2 = ResolveAudioGuid(guid);
					if (audio2.HasValue)
						AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{audio2}}}");
					break;
				}
		}
	}

	private static void EmitPlaylistChild(XmlDocument xmlDoc, XmlElement root, FModGuid childGuid, FModReader bank, string eventPath, HashSet<string> emitted)
	{
		string key = childGuid.ToString();
		if (!emitted.Add(key))
			return;
		if (!AllInstrumentNodes.TryGetValue(childGuid, out BaseInstrumentNode? child))
		{
			SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{childGuid}}}", out XmlElement el);
			Guid? a0 = ResolveAudioGuid(childGuid);
			if (a0.HasValue)
				AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{a0}}}");
			else
				PushToConsoleLog($"WARNING: Could not resolve playlist child {childGuid} in event {eventPath}", YELLOW);
			return;
		}
		List<ModulatorNode> mods = [.. bank.ModulatorNodes.Values.Where(m => m.OwnerGuid.ToString() == key)];
		switch (child)
		{
			case WaveformInstrumentNode:
				{
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{childGuid}}}", out XmlElement el);
					AttachModulators(el, mods);
					EmitModulatorObjects(xmlDoc, root, mods);
					Guid? a = ResolveAudioGuid(childGuid);
					if (a.HasValue)
						AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{a}}}");
					else
						PushToConsoleLog($"WARNING: Could not resolve audio file for playlist child {childGuid} in event {eventPath}", YELLOW);
					break;
				}
			case MultiInstrumentNode multi:
				{
					SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{childGuid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isAsync", "true");
					AttachModulators(el, mods);
					EmitModulatorObjects(xmlDoc, root, mods);
					if (multi.PlaylistBody?.Entries != null && multi.PlaylistBody.Entries.Length > 0)
					{
						AddMultiRelationshipElement(xmlDoc, el, "sounds", multi.PlaylistBody.Entries.Select(en => en.Guid.ToGuid()).ToArray());
						foreach (var en in multi.PlaylistBody.Entries)
							EmitPlaylistChild(xmlDoc, root, en.Guid, bank, eventPath, emitted);
					}
					break;
				}
			case ScattererInstrumentNode scatter:
				{
					SetupHeaderXML(xmlDoc, root, "SoundScatterer", $"{{{childGuid}}}", out XmlElement el);
					AttachModulators(el, mods);
					EmitModulatorObjects(xmlDoc, root, mods);
					if (scatter.PlaylistBody?.Entries != null && scatter.PlaylistBody.Entries.Length > 0)
					{
						AddMultiRelationshipElement(xmlDoc, el, "sounds", scatter.PlaylistBody.Entries.Select(en => en.Guid.ToGuid()).ToArray());
						foreach (var en in scatter.PlaylistBody.Entries)
							EmitPlaylistChild(xmlDoc, root, en.Guid, bank, eventPath, emitted);
					}
					break;
				}
			case ProgrammerInstrumentNode:
				{
					SetupHeaderXML(xmlDoc, root, "ProgrammerSound", $"{{{childGuid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isAsync", "true");
					Guid placeholder2 = AudioFile.CreatePlaceholderAudioFile(EventsPlaceholderFolder);
					AddRelationshipElement(xmlDoc, el, "placeholder", $"{{{placeholder2}}}");
					break;
				}
			case SilenceInstrumentNode s:
				{
					SetupHeaderXML(xmlDoc, root, "SilenceSound", $"{{{childGuid}}}", out XmlElement el);
					if (s.Duration != 0)
						AddPropertyElement(xmlDoc, el, "length", $"{s.Duration}");
					break;
				}
			case EventInstrumentNode e:
				{
					SetupHeaderXML(xmlDoc, root, "EventSound", $"{{{childGuid}}}", out XmlElement el);
					AddRelationshipElement(xmlDoc, el, "event", $"{{{e.EventGuid}}}");
					break;
				}
			default:
				{
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{childGuid}}}", out XmlElement el);
					Guid? a2 = ResolveAudioGuid(childGuid);
					if (a2.HasValue)
						AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{a2}}}");
					break;
				}
		}
	}

	// Emits a sound placed on a parameter sheet, where start/length are parameter values.
	private static void EmitParameterSound(XmlDocument xmlDoc, XmlElement root, FModGuid guid, float rangeStart, float rangeEnd, FModReader bank, string eventPath, HashSet<string> emitted)
	{
		string key = guid.ToString();
		if (!emitted.Add(key)) return;

		if (!AllInstrumentNodes.TryGetValue(guid, out BaseInstrumentNode? instrument))
		{
			SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement el);
			AddPropertyElement(xmlDoc, el, "isCutoff", "true");
			AddPropertyElement(xmlDoc, el, "start", $"{rangeStart}");
			AddPropertyElement(xmlDoc, el, "length", $"{rangeEnd - rangeStart}");
			return;
		}

		switch (instrument)
		{
			case MultiInstrumentNode multi:
				{
					SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{guid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isCutoff", "true");
					AddPropertyElement(xmlDoc, el, "start", $"{rangeStart}");
					AddPropertyElement(xmlDoc, el, "length", $"{rangeEnd - rangeStart}");
					AddPropertyElement(xmlDoc, el, "isAsync", "true");
					if (multi.PlaylistBody?.Entries is { Length: > 0 })
					{
						AddMultiRelationshipElement(xmlDoc, el, "sounds", multi.PlaylistBody.Entries.Select(en => en.Guid.ToGuid()).ToArray());
						foreach (var en in multi.PlaylistBody.Entries)
							EmitPlaylistChild(xmlDoc, root, en.Guid, bank, eventPath, emitted);
					}
					break;
				}
			case ScattererInstrumentNode:
				{
					SetupHeaderXML(xmlDoc, root, "SoundScatterer", $"{{{guid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isCutoff", "true");
					AddPropertyElement(xmlDoc, el, "start", $"{rangeStart}");
					AddPropertyElement(xmlDoc, el, "length", $"{rangeEnd - rangeStart}");
					break;
				}
			default:
				{
					SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{guid}}}", out XmlElement el);
					AddPropertyElement(xmlDoc, el, "isCutoff", "true");
					AddPropertyElement(xmlDoc, el, "start", $"{rangeStart}");
					AddPropertyElement(xmlDoc, el, "length", $"{rangeEnd - rangeStart}");
					Guid? audio = ResolveAudioGuid(guid);
					if (audio.HasValue)
						AddRelationshipElement(xmlDoc, el, "audioFile", $"{{{audio}}}");
					break;
				}
		}
	}

	private static void AttachModulators(XmlElement parent, List<ModulatorNode> mods)
	{
		if (mods.Count == 0)
			return;
		// Caller must have xmlDoc/root; attach via relationship using parent owner doc
		var xmlDoc = parent.OwnerDocument;
		var rel = xmlDoc.CreateElement("relationship");
		rel.SetAttribute("name", "modulators");
		foreach (var m in mods)
		{
			var dest = xmlDoc.CreateElement("destination");
			dest.InnerText = $"{{{m.BaseGuid}}}";
			rel.AppendChild(dest);
		}
		parent.AppendChild(rel);
	}

	private static void EmitModulatorObjects(XmlDocument xmlDoc, XmlElement root, List<ModulatorNode> mods)
	{
		foreach (var mod in mods)
		{
			string k = mod.BaseGuid.ToString();
			// An instrument can be reached via both a timeline trigger box and a playlist entry, only define each modulator object once.
			if (emittedModulators.Add(k))
				EmitModulator(xmlDoc, root, mod);
		}
	}

	private static Guid? ResolveAudioGuid(FModGuid instrumentGuid)
	{
		List<FModGuid> wavResourceGuids = ResolveWavResourceGuids(instrumentGuid);
		foreach (var wavResGuid in wavResourceGuids)
		{
			if (WavGUIDs.TryGetValue(wavResGuid, out Guid audioGuid))
				return audioGuid;
		}
		if (WavGUIDs.TryGetValue(instrumentGuid, out Guid direct))
			return direct;
		return null;
	}

	private static List<FModGuid> ResolveWavResourceGuids(FModGuid instrumentGuid, HashSet<string>? visited = null)
	{
		List<FModGuid> results = [];
		visited ??= [];
		string key = instrumentGuid.ToString();
		if (!visited.Add(key)) return results;
		if (!AllInstrumentNodes.TryGetValue(instrumentGuid, out BaseInstrumentNode? instrument)) return results;

		if (instrument.InstrumentBody?.TimelineGuid is not null && instrument.InstrumentBody.TimelineGuid != default(FModGuid) && !instrument.InstrumentBody.TimelineGuid.IsEmpty)
			results.AddRange(ResolveWavResourceGuids(instrument.InstrumentBody.TimelineGuid, visited));

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

	public static string GetHigherEventFolder(string EventPath)
	{
		List<string> folders = SplitEventPath(EventPath);
		if (folders.Count >= 1)
		{
			string folderKey = string.Join("/", folders);
			return $"{EventFolderGUIDs[folderKey]}";
		}
		return $"{MasterEventFolderGUID}";
	}

	public static string GetName(string EventPath)
	{
		List<string> folders = [.. EventPath.Split('/')];
		return $"{folders[^1]}";
	}

	private static string GetMixerGroupName(int index) => $"Track {index}";

	private static void EmitTriggerConditions(XmlDocument xmlDoc, XmlElement root, XmlElement owner, List<FEvaluator>? evaluators)
	{
		if (evaluators is null || evaluators.Count == 0)
			return;

		string? paramGuid = null;
		float[]? range = null;
		foreach (var evaluator in evaluators)
		{
			if (evaluator.Data is null) continue;
			if ((int)evaluator.Type == (int)EEvaluatorType.Type11)
				paramGuid = $"{evaluator.Data}";
			else if ((int)evaluator.Type == (int)EEvaluatorType.Type20 && evaluator.Data is Array arr)
				range = arr.Cast<object>().Select(o => Convert.ToSingle(o, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
		}

		if (paramGuid is null || range is null || range.Length < 2)
			return;

		Guid conditionGuid = GetRandomGUID();
		SetupHeaderXML(xmlDoc, root, "ParameterCondition", $"{{{conditionGuid}}}", out XmlElement conditionElement);
		AddPropertyElement(xmlDoc, conditionElement, "minimum", $"{range[0]}");
		AddPropertyElement(xmlDoc, conditionElement, "maximum", $"{range[1]}");
		AddRelationshipElement(xmlDoc, conditionElement, "parameter", $"{{{paramGuid}}}");
		AddMultiRelationshipElement(xmlDoc, owner, "triggerConditions", [conditionGuid]);
	}

	/*
		Command instruments have no recoverable XML schema, so they are excluded from the
		project rather than emitted with guessed property names.
	*/
	private static bool IsUnrecoverableInstrument(FModGuid guid)
		=> AllInstrumentNodes.TryGetValue(guid, out BaseInstrumentNode? inst) && inst is CommandInstrumentNode;

	// "event:/Character/Radio Dialogue" -> "Character" (folder path without the event name)
	public static string GetFolderPath(string EventPath)
	{
		List<string> parts = EventPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
		if (parts.Count > 0)
			parts.RemoveAt(0); // drop "event:"
		if (parts.Count > 0)
			parts.RemoveAt(parts.Count - 1); // drop the event name
		return parts.Count > 0 ? string.Join("/", parts) : "";
	}

	/*
		Original projects only ever use pitch/volume/startOffset/snapshotIntensity, never "normal".
		The bank encodes the property as (PropertyType, PropertyIndex), the index table below was verified against the example project
	*/
	private static string GetModulatedPropertyName(ModulatorNode Mod, bool ownerIsSnapshot)
	{
		if ((uint)Mod.PropertyType == 1)
			return "volume";

		// normal type, resolve via property index.
		int idx = Mod.PropertyIndex;
		if (ownerIsSnapshot)
		{
			// Snapshots expose snapshotIntensity at index 3 (good against example project)
			if (idx == 3) return "snapshotIntensity";
			return "snapshotIntensity";
		}
		return idx switch
		{
			1 => "pitch",
			2 => "pan",
			3 => "lowpass",
			4 => "highpass",
			5 => "sendLevel",
			11 => "startOffset",
			// Unmapped index, "volume" is the one automatable property guaranteed to exist on every object, so this stays valid rather than naming a property that doesn't exist.
			_ => "volume",
		};
	}

	private static void EmitModulator(XmlDocument xmlDoc, XmlElement root, ModulatorNode Mod, bool ownerIsSnapshot = false)
	{
		string ModPropType = GetModulatedPropertyName(Mod, ownerIsSnapshot);
		/*
			PropertyIndex disambiguates pitch (commonly index 2 i think?) preserve volume default, map pitch when detected Bank EPropertyType has no pitch, pitch modulators appear as Normal with specific index.
			The banks and example project i used as a reference, the original Pistol uses pitch+volume randomizers, map Normal to pitch for instrument modulators as best as possible
		*/
		switch ((int)Mod.Type)
		{
			case 0:
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
			case 1:
				{
					RandomModulatorNode? SubNode = (RandomModulatorNode?)Mod.Subnode;
					if (SubNode is null) break;
					SetupHeaderXML(xmlDoc, root, "RandomizerModulator", $"{{{Mod.BaseGuid}}}", out XmlElement RandomizerModulatorElement);
					AddPropertyElement(xmlDoc, RandomizerModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
					if (SubNode.Amount != 0)
						AddPropertyElement(xmlDoc, RandomizerModulatorElement, "amount", $"{SubNode.Amount}");
					break;
				}
			case 2:
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
			case 3:
				{
					LFOModulatorNode? SubNode = (LFOModulatorNode?)Mod.Subnode;
					if (SubNode is null) break;
					SetupHeaderXML(xmlDoc, root, "LFOModulator", $"{{{Mod.BaseGuid}}}", out XmlElement LFOModulatorElement);
					AddPropertyElement(xmlDoc, LFOModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
					if (SubNode.Shape != 0)
						AddPropertyElement(xmlDoc, LFOModulatorElement, "shape", $"{SubNode.Shape}");
					if (SubNode.Rate != 0.50)
						AddPropertyElement(xmlDoc, LFOModulatorElement, "rate", $"{SubNode.Rate}");
					if (SubNode.Phase != 0)
						AddPropertyElement(xmlDoc, LFOModulatorElement, "phase", $"{SubNode.Phase}");
					AddPropertyElement(xmlDoc, LFOModulatorElement, "depth", $"{SubNode.Amount}");
					AddPropertyElement(xmlDoc, LFOModulatorElement, "direction", $"{SubNode.Direction}");
					break;
				}
			case 4:
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
			case 5:
				{
					SpectralSidechainModulatorNode? SubNode = (SpectralSidechainModulatorNode?)Mod.Subnode;
					if (SubNode is null) break;
					SetupHeaderXML(xmlDoc, root, "SidechainModulator", $"{{{Mod.BaseGuid}}}", out XmlElement SidechainModulatorElement);
					AddPropertyElement(xmlDoc, SidechainModulatorElement, "nameOfPropertyBeingModulated", ModPropType);
					if (SubNode.Mode == 0)
						AddPropertyElement(xmlDoc, SidechainModulatorElement, "levelMode", "1");
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
					if (SubNode.ThresholdMapping != default)
						AddRelationshipElement(xmlDoc, SidechainModulatorElement, "sidechain", $"{{{SubNode.ThresholdMapping}}}");
					break;
				}
		}
	}
}
