using FModBankParser;
using FModBankParser.Nodes.Buses;
using FModBankParser.Objects;
using System.Xml;
using static Program;
using static XMLHelper;

// Create Main XML Files found in /Metadata
// These will use Template ones, since we can't get information of more complex stuff from the .bank files
public class MasterXMLs
{
	#region XML Files that are in their own subfolders
	public static void Create_MasterAssetXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterAssetFolder", $"{{{MasterAssetsGUID}}}", out XmlElement Element);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Asset/{{{MasterAssetsGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_MasterBankFoldersXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterBankFolder", $"{{{MasterBankFolderGUID}}}", out XmlElement Element);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/BankFolder/{{{MasterBankFolderGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_MasterBankXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "Bank", $"{{{MasterBankGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "name", "Master");
		AddPropertyElement(xmlDoc, Element, "isMasterBank", "true");
		AddRelationshipElement(xmlDoc, Element, "folder", $"{{{MasterBankFolderGUID}}}");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Bank/{{{MasterBankGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_EventFolderXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterEventFolder", $"{{{MasterEventFolderGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "name", "Master");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/EventFolder/{{{MasterEventFolderGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_PlatformXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "Platform", $"{{{MasterPlatformGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "hardwareType", "0");
		AddPropertyElement(xmlDoc, Element, "name", "Desktop");
		AddPropertyElement(xmlDoc, Element, "subDirectory", "Desktop");
		AddPropertyElement(xmlDoc, Element, "speakerFormat", "5");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Platform/{{{MasterPlatformGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_EncodingSettingXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "EncodingSetting", $"{{{MasterEncodingSettingGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "encodingFormat", "3");
		AddPropertyElement(xmlDoc, Element, "quality", "37");
		AddRelationshipElement(xmlDoc, Element, "platform", $"{{{MasterPlatformGUID}}}");
		AddRelationshipElement(xmlDoc, Element, "encodable", $"{{{MasterPlatformGUID}}}");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/EncodingSetting/{{{MasterEncodingSettingGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_EffectPresetFolderXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterEffectPresetFolder", $"{{{MasterEffectPresetGUID}}}", out XmlElement Element);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/EffectPresetFolder/{{{MasterEffectPresetGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_ParameterPresetFolderXML()
	{
		// Master parameter preset folder (referenced by Workspace.xml)
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		SetupHeaderXML(xmlDoc, root, "MasterParameterPresetFolder", $"{{{MasterParameterPresetGUID}}}", out _);
		xmlDoc.AppendChild(root);
		SaveXML(xmlDoc, outputProjectPath + $"/Metadata/ParameterPresetFolder/{{{MasterParameterPresetGUID}}}.xml");

		/*
			Concrete folder that the presets live in
			In the reference project every preset sits inside a real ParameterPresetFolder, never directly under the master folder, so, the shape is reproduced here.
		*/
		SetupXML(out XmlDocument xmlDoc2, out XmlElement root2);
		SetupHeaderXML(xmlDoc2, root2, "ParameterPresetFolder", $"{{{ParameterPresetFolderGUID}}}", out XmlElement FolderElement);
		AddPropertyElement(xmlDoc2, FolderElement, "name", "Presets");
		AddRelationshipElement(xmlDoc2, FolderElement, "folder", $"{{{MasterParameterPresetGUID}}}");
		xmlDoc2.AppendChild(root2);
		SaveXML(xmlDoc2, outputProjectPath + $"/Metadata/ParameterPresetFolder/{{{ParameterPresetFolderGUID}}}.xml");
	}
	public static void Create_ProfilerFolderXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "ProfilerSessionFolder", $"{{{MasterProfilerFolderGUID}}}", out XmlElement Element);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/ProfilerFolder/{{{MasterProfilerFolderGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	public static void Create_SandboxFolderXML() 
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterSandboxFolder", $"{{{MasterSandboxFolderGUID}}}", out XmlElement Element);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/SandboxFolder/{{{MasterSandboxFolderGUID}}}.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
#endregion

	#region XML Files in the /Metadata folder
	public static void Create_MasterXML()
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MixerMaster", $"{{{MasterXMLGUID}}}", out XmlElement MixerMasterElement);
		AddPropertyElement(xmlDoc, MixerMasterElement, "name", "Master Bus");
		AddRelationshipElement(xmlDoc, MixerMasterElement, "effectChain", $"{{{Master1GUID}}}");
		AddRelationshipElement(xmlDoc, MixerMasterElement, "panner", $"{{{Master2GUID}}}");
		AddRelationshipElement(xmlDoc, MixerMasterElement, "mixer", $"{{{MasterMixerXMLGUID}}}");

		SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{Master1GUID}}}", out XmlElement MixerBusEffectChainElement);
		AddRelationshipElement(xmlDoc, MixerBusEffectChainElement, "effects", $"{{{Master3GUID}}}");

		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{Master2GUID}}}", out XmlElement MixerBusPannerElement);
		AddPropertyElement(xmlDoc, MixerBusPannerElement, "overridingOutputFormat", "2");

		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{Master3GUID}}}", out XmlElement MixerBusFaderElement);

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Master.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}

	public static void Create_MixerXML()
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "Mixer", $"{{{MasterMixerXMLGUID}}}", out XmlElement MixerElement);
		AddRelationshipElement(xmlDoc, MixerElement, "masterBus", $"{{{MasterXMLGUID}}}");
		AddRelationshipElement(xmlDoc, MixerElement, "snapshotList", $"{{{MasterSnapshotGUID}}}");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Mixer.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}

	public static void Create_TagsXML()
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "MasterTagFolder", $"{{{MasterTagsXMLGUID}}}", out XmlElement MasterTagElement);
		AddPropertyElement(xmlDoc, MasterTagElement, "name", "Master");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Tags.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}

	public static void Create_WorkspaceXML()
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "Workspace", $"{{{MasterWorkspaceXMLGUID}}}", out XmlElement WorkspaceElement);
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterEventFolder", $"{{{MasterEventFolderGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterTagFolder", $"{{{MasterTagsXMLGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterEffectPresetFolder", $"{{{MasterEffectPresetGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterParameterPresetFolder", $"{{{MasterParameterPresetGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterBankFolder", $"{{{MasterBankFolderGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterSandboxFolder", $"{{{MasterSandboxFolderGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "mixer", $"{{{MasterMixerXMLGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "profilerSessionFolder", $"{{{MasterProfilerFolderGUID}}}");
		AddRelationshipElement(xmlDoc, WorkspaceElement, "platforms", $"{{{MasterPlatformGUID}}}");

		xmlDoc.AppendChild(root);

		// XML File Path
		string filePath = outputProjectPath + $"/Metadata/Workspace.xml";

		// Save
		SaveXML(xmlDoc, filePath);
	}
	#endregion

	#region XML Files per each Bank File
	public static void Create_BankFileXML(FModGuid BankGUID, string BankName, bool isMasterBank = false, bool isAudioTable = false, string? audioTableSourceDir = null)
	{
		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "Bank", $"{{{BankGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "name", BankName);
		if (isMasterBank)
			AddPropertyElement(xmlDoc, Element, "isMasterBank", "true");

		if (isAudioTable)
		{
			// Audio tables (localized dialogue) keep their files loose on disk instead of in /Assets.
			Guid tableGuid = GetRandomGUID();
			AddRelationshipElement(xmlDoc, Element, "audioTable", $"{{{tableGuid}}}");
			AddRelationshipElement(xmlDoc, Element, "folder", $"{{{MasterBankFolderGUID}}}");

			SetupHeaderXML(xmlDoc, root, "AudioTable", $"{{{tableGuid}}}", out XmlElement TableElement);
			AddPropertyElement(xmlDoc, TableElement, "name", "Audio Table");
			AddPropertyElement(xmlDoc, TableElement, "sourceDirectory", audioTableSourceDir ?? BankName);
			AddPropertyElement(xmlDoc, TableElement, "isLocalized", "true");
		}
		else
			AddRelationshipElement(xmlDoc, Element, "folder", $"{{{MasterBankFolderGUID}}}");

		xmlDoc.AppendChild(root);

		// Save
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Bank/{{{BankGUID}}}.xml");
	}

	//Connects stuff like Audio Files to their original Bank File
	public static void Create_BankAssetXML(string BankName)
	{
		Guid BankGUID = GetRandomGUID();

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		SetupHeaderXML(xmlDoc, root, "EncodableAsset", $"{{{BankGUID}}}", out XmlElement Element);
		AddPropertyElement(xmlDoc, Element, "assetPath", BankName);
		AddRelationshipElement(xmlDoc, Element, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");

		xmlDoc.AppendChild(root);

		// Save
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Asset/{{{BankGUID}}}.xml");
	}

	/*
		Standalone mixer buses.
		Every event owns its own EventMixerMaster/EventMixerGroup/EventMixerInput, but the banks
		also contain the global mixer tree (the "Mixer" window). Those buses are the ones nothing
		else in the project references, and snapshots animate them directly - so they must exist
		or every SnapshotProperty/SnapshotTrack ends up pointing at nothing.
		Names are not stored in the bank, so the buses are emitted unnamed.
	*/
	public static void Create_StandaloneMixerBuses()
	{
		int busDisplayName = 0;
		foreach (var kvp in AllBusNodes)
		{
			string busKey = kvp.Key.ToString();
			if (ReferencedBusGuids.Contains(busKey))
				continue;

			var bus = kvp.Value;
			Guid busGuid = bus.BaseGuid.ToGuid();
			Guid ecGuid = GetRandomGUID();
			Guid pannerGuid = GetRandomGUID();
			Guid faderGuid = GetRandomGUID();
			// Bus names are authored in the project, not stored in the bank (FRoutable has no
			// name field), so a stable placeholder is used - Studio always writes one.
			busDisplayName++;
			string busName = bus switch
			{
				ReturnBusNode => $"Return {busDisplayName}",
				MasterBusNode => "Master Bus",
				_ => $"Bus {busDisplayName}",
			};

			SetupXML(out XmlDocument xmlDoc, out XmlElement root);

			switch (bus)
			{
				case ReturnBusNode:
					{
						// MixerReturn lives on its own; ReturnTrack is what buses actually send to.
						SetupHeaderXML(xmlDoc, root, "MixerReturn", $"{{{busGuid}}}", out XmlElement ret);
						// FMixerStrip.Volume is the authored bus level and is recoverable.
						float? retVolume = (bus as ReturnBusNode) is { } rb ? rb.BusBody.MixerStrip.Volume : null;
						if (retVolume.HasValue && Math.Abs(retVolume.Value) > 0.001f)
							AddPropertyElement(xmlDoc, ret, "volume", $"{retVolume.Value}");
						AddPropertyElement(xmlDoc, ret, "name", busName);
						AddRelationshipElement(xmlDoc, ret, "effectChain", $"{{{ecGuid}}}");
						AddRelationshipElement(xmlDoc, ret, "panner", $"{{{pannerGuid}}}");
						AddRelationshipElement(xmlDoc, ret, "output", $"{{{EventOutputBusGUID}}}");
						break;
					}
				case MasterBusNode:
					{
						SetupHeaderXML(xmlDoc, root, "MixerMaster", $"{{{busGuid}}}", out XmlElement mast);
						AddRelationshipElement(xmlDoc, mast, "effectChain", $"{{{ecGuid}}}");
						AddRelationshipElement(xmlDoc, mast, "panner", $"{{{pannerGuid}}}");
						AddRelationshipElement(xmlDoc, mast, "mixer", $"{{{MasterMixerXMLGUID}}}");
						break;
					}
				default:
					{
						SetupHeaderXML(xmlDoc, root, "MixerGroup", $"{{{busGuid}}}", out XmlElement grp);
						AddPropertyElement(xmlDoc, grp, "name", busName);
						AddRelationshipElement(xmlDoc, grp, "effectChain", $"{{{ecGuid}}}");
						AddRelationshipElement(xmlDoc, grp, "panner", $"{{{pannerGuid}}}");
						AddRelationshipElement(xmlDoc, grp, "output", $"{{{MasterXMLGUID}}}");
						break;
					}
			}

			SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{ecGuid}}}", out XmlElement ec);
			AddRelationshipElement(xmlDoc, ec, "effects", $"{{{faderGuid}}}");
			SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{pannerGuid}}}", out _);
			SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{faderGuid}}}", out _);

			xmlDoc.AppendChild(root);

			string subFolder = bus is ReturnBusNode ? "Return" : (bus is MasterBusNode ? "" : "Group");
			string filePath = subFolder.Length > 0
				? $"{outputProjectPath}/Metadata/{subFolder}/{{{busGuid}}}.xml"
				: $"{outputProjectPath}/Metadata/Master{{{busGuid}}}.xml";
			if (subFolder.Length == 0)
			{
				// The real master bus lives in Master.xml; merge into that file instead.
				continue;
			}
			SaveXML(xmlDoc, filePath);
		}

		Create_EventOutputBus();
	}

	/*
		Studio never routes an event's MixerInput straight at the MixerMaster - in the
		reference project all 25 events send to a MixerGroup in the mixer tree, and the
		MixerReturn does too. Which mixer bus an event was assigned to is a project-level
		routing decision that the bank does not store (the bank only holds the event's own
		MasterBusNode/InputBusNode/GroupBusNodes), so rather than guess a bus per event every
		event is routed into one catch-all group that feeds the master bus. That keeps the
		mixer tree shape Studio writes without inventing per-event routing.
	*/
	private static void Create_EventOutputBus()
	{
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);

		Guid ecGuid = GetRandomGUID();
		Guid pannerGuid = GetRandomGUID();
		Guid faderGuid = GetRandomGUID();

		SetupHeaderXML(xmlDoc, root, "MixerGroup", $"{{{EventOutputBusGUID}}}", out XmlElement grp);
		AddPropertyElement(xmlDoc, grp, "name", "Events");
		AddRelationshipElement(xmlDoc, grp, "effectChain", $"{{{ecGuid}}}");
		AddRelationshipElement(xmlDoc, grp, "panner", $"{{{pannerGuid}}}");
		AddRelationshipElement(xmlDoc, grp, "output", $"{{{MasterXMLGUID}}}");

		SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{ecGuid}}}", out XmlElement ec);
		AddRelationshipElement(xmlDoc, ec, "effects", $"{{{faderGuid}}}");
		SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{pannerGuid}}}", out _);
		SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{faderGuid}}}", out _);

		xmlDoc.AppendChild(root);
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/Group/{{{EventOutputBusGUID}}}.xml");
	}
	#endregion

	// Creates the .fspro file with all object references
	public static void Create_FSPROFile(string projectname)
	{
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		// Add the workspace reference (main entry point)
		SetupHeaderXML(xmlDoc, root, "Workspace", $"{{{MasterWorkspaceXMLGUID}}}", out _);

		// Save
		SaveXML(xmlDoc, $"{outputProjectPath}/{projectname}.fspro");
	}
}