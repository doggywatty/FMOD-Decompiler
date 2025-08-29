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
        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "MasterParameterPresetFolder", $"{{{MasterParameterPresetGUID}}}", out XmlElement Element);

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + $"/Metadata/ParameterPresetFolder/{{{MasterParameterPresetGUID}}}.xml";

        // Save
        SaveXML(xmlDoc, filePath);
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
    public static void Create_SnapshotGroupXML() 
    {
        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "SnapshotList", $"{{{MasterSnapshotGUID}}}", out XmlElement Element);
        AddRelationshipElement(xmlDoc, Element, "mixer", $"{{{MasterMixerXMLGUID}}}");

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + $"/Metadata/SnapshotGroup/{{{MasterSandboxFolderGUID}}}.xml";

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
    public static void Create_BankFileXML(string bankfilename)
    {
        var BankGUID = BankSpecificGUIDs[bankfilename + "_Bank"];
        var BankName = bankfilename.Replace(".bank", "");

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "Bank", $"{{{BankGUID}}}", out XmlElement Element);
        AddPropertyElement(xmlDoc, Element, "name", BankName);
        AddRelationshipElement(xmlDoc, Element, "folder", $"{{{MasterBankFolderGUID}}}");

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + $"/Metadata/Bank/{{{BankGUID}}}.xml";

        // Save
        SaveXML(xmlDoc, filePath);
    }

    //Connects stuff like Audio Files to their original Bank File
    public static void Create_BankAssetXML(string bankfilename)
    {
        var BankGUID = BankSpecificGUIDs[bankfilename + "_Asset"];
        var BankName = bankfilename.Replace(".bank", "/");// Replace Music.bank to be Music/ (to set it as a valid folder)

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);

        SetupHeaderXML(xmlDoc, root, "EncodableAsset", $"{{{BankGUID}}}", out XmlElement Element);
        AddPropertyElement(xmlDoc, Element, "assetPath", BankName);
        AddRelationshipElement(xmlDoc, Element, "masterAssetFolder", $"{{{MasterAssetsGUID}}}");

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + $"/Metadata/Asset/{{{BankGUID}}}.xml";

        // Save
        SaveXML(xmlDoc, filePath);
    }
    #endregion

    // Creates the .fspro file
    public static void Create_FSPROFile(string projectname)
    {
        // it literally just contains setup header
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + $"/{projectname}.fspro";

        // Save
        SaveXML(xmlDoc, filePath);
    }
}