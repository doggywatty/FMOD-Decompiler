using static Program;

// Create Main XML Files found in /Metadata
// These will use Template ones, since we can't get information of more complex stuff from the .bank files
public class MasterXMLs
{ 
    public static void Create_MasterXML(string filepath)
    {
        File.WriteAllText(filepath + $"/Metadata/Master.xml", ""
            + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r"
            + "\n<objects serializationModel=\"Studio.02.02.00\">\r"
            + $"\n\t<object class=\"MixerMaster\" id=\"{{{MasterXMLGUID}}}\">\r"
            + "\n\t\t<property name=\"name\">\r\n\t\t\t<value>Master Bus</value>\r"
            + "\n\t\t</property>\r\n\t\t<relationship name=\"effectChain\">\r"
            + $"\n\t\t\t<destination>{{{Master1GUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t\t<relationship name=\"panner\">\r"
            + $"\n\t\t\t<destination>{{{Master2GUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t\t<relationship name=\"mixer\">\r"
            + $"\n\t\t\t<destination>{{{MasterMixerXMLGUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t</object>\r"
            + $"\n\t<object class=\"MixerBusEffectChain\" id=\"{{{Master1GUID}}}\">\r"
            + "\n\t\t<relationship name=\"effects\">\r"
            + $"\n\t\t\t<destination>{{{Master3GUID}}}</destination>\r\n\t\t</relationship>\r"
            + $"\n\t</object>\r\n\t<object class=\"MixerBusPanner\" id=\"{{{Master2GUID}}}\">\r"
            + "\n\t\t<property name=\"overridingOutputFormat\">\r\n\t\t\t<value>2</value>\r\n\t\t</property>\r"
            + $"\n\t</object>\r\n\t<object class=\"MixerBusFader\" id=\"{{{Master3GUID}}}\" />\r\n</objects>");
    }

    public static void Create_MixerXML(string filepath)
    {
        File.WriteAllText(filepath + $"/Metadata/Mixer.xml", ""
            + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<objects serializationModel=\"Studio.02.02.00\">\r"
            + $"\n\t<object class=\"Mixer\" id=\"{{{MasterMixerXMLGUID}}}\">\r\n\t\t<relationship name=\"masterBus\">\r"
            + $"\n\t\t\t<destination>{{{MasterXMLGUID}}}</destination>\r\n\t\t</relationship>\r"
            + $"\n\t\t<relationship name=\"snapshotList\">\r\n\t\t\t<destination>{{{MasterSnapshotGUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t</object>\r\n</objects>");
    }

    public static void Create_TagsXML(string filepath)
    {
        File.WriteAllText(filepath + $"/Metadata/Tags.xml", ""
            + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<objects serializationModel=\"Studio.02.02.00\">\r"
            + $"\n\t<object class=\"MasterTagFolder\" id=\"{{{MasterTagsXMLGUID}}}\">\r\n\t\t<property name=\"name\">\r"
            + "\n\t\t\t<value>Master</value>\r\n\t\t</property>\r\n\t</object>\r\n</objects>");
    }

    public static void Create_WorkspaceXML(string filepath)
    {
        File.WriteAllText(filepath + $"/Metadata/Workspace.xml", ""
            + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<objects serializationModel=\"Studio.02.02.00\">\r"
            + $"\n\t<object class=\"Workspace\" id=\"{{{MasterWorkspaceXMLGUID}}}\">\r\n\t\t<relationship name=\"masterEventFolder\">\r"
            + $"\n\t\t\t<destination>{{{MasterEventFolderGUID}}}</destination>\r\n\t\t</relationship>\r"
            + $"\n\t\t<relationship name=\"masterTagFolder\">\r\n\t\t\t<destination>{{{MasterTagsXMLGUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t\t<relationship name=\"masterEffectPresetFolder\">\r"
            + $"\n\t\t\t<destination>{{{MasterEffectPresetGUID}}}</destination>\r\n\t\t</relationship>\r"
            + $"\n\t\t<relationship name=\"masterParameterPresetFolder\">\r\n\t\t\t<destination>{{{MasterParameterPresetGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"masterBankFolder\">\r\n\t\t\t<destination>{{{MasterBankFolderGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"masterSandboxFolder\">\r\n\t\t\t<destination>{{{MasterSandboxFolderGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"masterAssetFolder\">\r\n\t\t\t<destination>{{{MasterAssetsGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"mixer\">\r\n\t\t\t<destination>{{{MasterMixerXMLGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"profilerSessionFolder\">\r\n\t\t\t<destination>{{{MasterProfilerFolderGUID}}}</destination>\r"
            + $"\n\t\t</relationship>\r\n\t\t<relationship name=\"platforms\">\r\n\t\t\t<destination>{{{MasterPlatformGUID}}}</destination>\r"
            + "\n\t\t</relationship>\r\n\t</object>\r\n</objects>");
    }
}