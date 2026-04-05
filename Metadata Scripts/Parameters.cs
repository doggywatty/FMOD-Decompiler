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

        AddPropertyElement(xmlDoc, ParamElement, "initialValue", $"{Param.DefaultValue}");
        if (Param.Minimum != 0)
            AddPropertyElement(xmlDoc, ParamElement, "minimum", $"{Param.Minimum}");
        if (Param.Maximum != 1)
            AddPropertyElement(xmlDoc, ParamElement, "maximum", $"{Param.Maximum}");

        // "0" == Continuious (is default and missing in XML)
        // "1" == Discrete
        // "2" == Labeled (requires some more stuff)

        if (Param.Labels.Length > 0) // Labeled
        {
            // mark as labelled
            AddPropertyElement(xmlDoc, ParamElement, "parameterType", "2");

            // add labels
            var propElement = xmlDoc.CreateElement("property");
            propElement.SetAttribute("name", "enumerationLabels");
            foreach (string Label in Param.Labels)
            {
                var labelElement = xmlDoc.CreateElement("value");
                labelElement.InnerText = Label;
                propElement.AppendChild(labelElement);
            }

            ParamElement.AppendChild(propElement);
        }
        // TODO - can Continuious or Discrete be determined?

        // TODO - idk default value
        AddPropertyElement(xmlDoc, ParamElement, "seekSpeed", $"{Param.SeekSpeed}"); // "1"
        AddPropertyElement(xmlDoc, ParamElement, "seekSpeedDescending", $"{Param.SeekSpeedDown}"); // "2"

        // idk what this is, probably not gonna be added
        //AddPropertyElement(xmlDoc, ParamElement, "isExposedRecursively", "false");
        #endregion

        xmlDoc.AppendChild(root);

        // Save
        SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/ParameterPreset/{{{XMLGUID}}}.xml");
    }
}