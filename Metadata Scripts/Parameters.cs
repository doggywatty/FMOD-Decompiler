using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Parameters
{
    public static void ParameterXML(ParameterNode Param)
    {
        Guid XMLGUID = GetRandomGUID();

        #region Preset Segement
        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
        SetupHeaderXML(xmlDoc, root, "ParameterPreset", $"{{{XMLGUID}}}", out XmlElement PresetElement);

        // Parmeter Name
        AddPropertyElement(xmlDoc, PresetElement, "name", Param.Name);
        // Link back to Master Parm GUID
        AddRelationshipElement(xmlDoc, PresetElement, "folder", $"{{{MasterParameterPresetGUID}}}");
        // Link to Parameter Settings in Second Segment below
        AddRelationshipElement(xmlDoc, PresetElement, "parameter", $"{{{Param.BaseGuid}}}");
        #endregion

        #region Parameter Segment
        SetupHeaderXML(xmlDoc, root, "GameParameter", $"{{{Param.BaseGuid}}}", out XmlElement ParamElement);

        // Main Properties
        AddPropertyElement(xmlDoc, ParamElement, "initialValue", $"{Param.DefaultValue}");
        if (Param.Minimum != 0)
            AddPropertyElement(xmlDoc, ParamElement, "minimum", $"{Param.Minimum}");
        if (Param.Maximum != 1)
            AddPropertyElement(xmlDoc, ParamElement, "maximum", $"{Param.Maximum}");

        #region Resolve Parameter Types
        string ParamFlags = (byte)Param.Flags switch
        {
            0x00 => "Continuous, Local",
            0x04 => "Continuous, Local", // in bo noise idk
            0x10 => "Continuous, Local, ReadOnly",
            0x02 => "Continuous, Local, IsHeld",
            0x12 => "Continuous, Local, ReadOnly, IsHeld",

            0x01 => "Continuous, Global",
            0x05 => "Continuous, Global", // in bo noise idk
            0x11 => "Continuous, Global, ReadOnly",
            0x03 => "Continuous, Global, IsHeld",
            0x13 => "Continuous, Global, ReadOnly, IsHeld",

            0x08 => "Discrete, Local",
            0x18 => "Discrete, Local, ReadOnly",
            0x0A => "Discrete, Local, IsHeld",
            0x1A => "Discrete, Local, ReadOnly, IsHeld",

            0x09 => "Discrete, Global",
            0x19 => "Discrete, Global, ReadOnly",
            0x0B => "Discrete, Global, IsHeld",
            0x1B => "Discrete, Global, ReadOnly, IsHeld",

            0x28 => "Labeled, Local",
            0x38 => "Labeled, Local, ReadOnly",
            0x2A => "Labeled, Local, IsHeld",
            0x3A => "Labeled, Local, ReadOnly, IsHeld",

            0x29 => "Labeled, Global",
            0x39 => "Labeled, Global, ReadOnly",
            0x2B => "Labeled, Global, IsHeld",
            0x3B => "Labeled, Global, ReadOnly, IsHeld",

            _ => "UNKNOWN",
        };

        int ParamType = (uint)Param.Type switch
        {
            0x1 => 3, // Distance
            0x4 => 4, // Direction
            0x5 => 5, // Elevation
            0x2 => 6, // Event Cone Angle
            0x3 => 7, // Event Orientation
            0x7 => 8, // Speed (Relative)
            0x8 => 9, // Speed (Absolute)
            0x9 => 10, // Distance (Normalized)

            // if GAME CONTROLLED, then we're not using this
            0x0 => -1, // Game Controlled
            _ => -1
        };
        #endregion
        #region XML Parameter Types
        // if Labeled
        if (Param.Labels.Length > 0 && ParamFlags.Contains("Labeled"))
        {
            // mark as labelled
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "2"); // "2" == Labeled

            // add labels
            AddMultiPropertyElement(xmlDoc, ParamElement, "enumerationLabels", Param.Labels);
        }
        // if Discrete
        else if (ParamFlags.Contains("Discrete"))
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "1"); // "1" == Discrete
        // if using Built-in Types
        else if (ParamType != -1)
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", $"{ParamType}");
        // if Continuious (or unknown)
        else
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "0"); // "0" == Continuious
        #endregion

        // Other Properties
        if (ParamFlags.Contains("Global")) 
            AddPropertyElement(xmlDoc, ParamElement, "isGlobal", "true");
        if (ParamFlags.Contains("ReadOnly")) 
            AddPropertyElement(xmlDoc, ParamElement, "isReadOnly", "true");
        if (ParamFlags.Contains("IsHeld"))
            AddPropertyElement(xmlDoc, ParamElement, "isHeld", "true");

        if (Param.Velocity != 0)
            AddPropertyElement(xmlDoc, ParamElement, "velocity", $"{Param.Velocity}");
        if (Param.SeekSpeed != 0)
            AddPropertyElement(xmlDoc, ParamElement, "seekSpeed", $"{Param.SeekSpeed}");
        if (Param.SeekSpeedDown != 0)
            AddPropertyElement(xmlDoc, ParamElement, "seekSpeedDescending", $"{Param.SeekSpeedDown}");

        // idk what this is, probably not gonna be added
        //AddPropertyElement(xmlDoc, ParamElement, "isExposedRecursively", "false");
        #endregion

        xmlDoc.AppendChild(root);

        // Save
        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/ParameterPreset/{{{XMLGUID}}}.xml");
    }
}