using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Parameters
{
    private struct ParamFlags
    {
        public string Type { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHeld { get; set; }

        public ParamFlags(string type)
        {
            Type = type;
            IsGlobal = false;
            IsReadOnly = false;
            IsHeld = false;
        }
    }

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
        ParamFlags ParamData = new((Param.Flags & 0x28) switch
        {
            0x20 or 0x28 => "Labeled",
            0x08 => "Discrete",
            _ => "Continuous"
        })
        {
            IsGlobal = (Param.Flags & 0x01) != 0,
            IsHeld = (Param.Flags & 0x02) != 0,
            IsReadOnly = (Param.Flags & 0x10) != 0
        };

        int BuiltinParamType = (uint)Param.Type switch
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
        if (Param.Labels.Length > 0 && ParamData.Type == "Labeled")
        {
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "2"); // "2" == Labeled
            AddMultiPropertyElement(xmlDoc, ParamElement, "enumerationLabels", Param.Labels); // add labels
        }
        // if Discrete
        else if (ParamData.Type == "Discrete")
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "1"); // "1" == Discrete
        // if using Built-in Types
        else if (BuiltinParamType != -1)
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", $"{BuiltinParamType}");
        // if Continuious (or unknown)
        else
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "0"); // "0" == Continuious
        #endregion

        // Other Properties
        if (ParamData.IsGlobal) 
            AddPropertyElement(xmlDoc, ParamElement, "isGlobal", "true");
        if (ParamData.IsReadOnly) 
            AddPropertyElement(xmlDoc, ParamElement, "isReadOnly", "true");
        if (ParamData.IsHeld)
            AddPropertyElement(xmlDoc, ParamElement, "isHeld", "true");

        if (Param.Velocity != 0)
            AddPropertyElement(xmlDoc, ParamElement, "velocity", $"{Param.Velocity}");
        if (Param.SeekSpeed != 0)
            AddPropertyElement(xmlDoc, ParamElement, "seekSpeed", $"{Param.SeekSpeed}");
        if (Param.SeekSpeedDown != 0)
            AddPropertyElement(xmlDoc, ParamElement, "seekSpeedDescending", $"{Param.SeekSpeedDown}");
        #endregion

        xmlDoc.AppendChild(root);

        // Save
        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/ParameterPreset/{{{XMLGUID}}}.xml");
    }
}