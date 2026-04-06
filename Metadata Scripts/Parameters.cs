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

        // "0" == Continuious (is default and missing in XML)
        // "1" == Discrete
        // "2" == Labeled (requires some more stuff)

        // if Labeled
        if (Param.Labels.Length > 0)
        {
            // mark as labelled
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "2");

            // add labels
            AddMultiPropertyElement(xmlDoc, ParamElement, "enumerationLabels", Param.Labels);
        }
        // if using Built-in Types
        else
        { 
            int realType = (uint)Param.Type switch
            {
                0x1 => 3, // Distance
                0x4 => 4, // Direction
                0x5 => 5, // Elevation
                0x2 => 6, // Event Cone Angle
                0x3 => 7, // Event Orientation
                0x7 => 8, // Speed (Relative)
                0x8 => 9, // Speed (Absolute)
                0x9 => 10, // Distance (Normalized)

                _ => 0 // default to Continuious

                // Listener Orientation is unknown, since Fmod Studio doesn't use it
                // it would probably be 11 though (if its even valid anymore)
            };

            AddPropertyElement(xmlDoc, ParamElement, "parameterType", $"{realType}");
        }
        // TODO - can Continuious or Discrete be determined?

        // Other Properties
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