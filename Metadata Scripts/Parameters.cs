using System.Xml;
using FMOD.Studio;
using static Program;
using static XMLHelper;

public class Parameters
{
    public static List<PARAMETER_DESCRIPTION> ParameterList = new List<PARAMETER_DESCRIPTION>();
    public static void ParameterXML(PARAMETER_DESCRIPTION parameter, EventDescription evDesc)
    {
        Guid XMLGUID = FMODGUIDToSysGuid(parameter.guid);
        Guid ParameterSettings = GetRandomGUID();

        // string name of the parameter
        string parmName = parameter.name;

        #region First Segment (Setup)
        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
        SetupHeaderXML(xmlDoc, root, "ParameterPreset", $"{{{XMLGUID}}}", out XmlElement objectElement1);

        // Parmeter Name
        AddPropertyElement(xmlDoc, objectElement1, "name", parmName);
        // Link back to Master Parm GUID
        AddRelationshipElement(xmlDoc, objectElement1, "folder", $"{{{MasterParameterPresetGUID}}}");
        // Link to Parameter Settings in Second Segment below
        AddRelationshipElement(xmlDoc, objectElement1, "parameter", $"{{{ParameterSettings}}}");
        #endregion

        #region Second Segment (Actual Shit)
        SetupHeaderXML(xmlDoc, root, "GameParameter", $"{{{ParameterSettings}}}", out XmlElement objectElement2);

        // Add Settings Elements
        // Initial Value is always there
        AddPropertyElement(xmlDoc, objectElement2, "initialValue", parameter.defaultvalue.ToString());

        // Minium Value
        // 0 is Default
        if (parameter.minimum != 0)
            AddPropertyElement(xmlDoc, objectElement2, "minimum", parameter.minimum.ToString());

        // Maximum Value
        // 1 is Default
        if (parameter.maximum != 1)
            AddPropertyElement(xmlDoc, objectElement2, "maximum", parameter.maximum.ToString());

        // Up here because the flags tell if its discrete
        // but we need to figure out if its labelled
        int parmType = 0;

        // Get Flag string and parse it for goodies
        string flag = parameter.flags.ToString();
        // If its not an integer (because it can sometimes be a zero for some reason
        if (flag != "0")
        {
            string[] flags = flag.Split(", ");

            // Check if isGlobal
            if (flags.Contains("GLOBAL") || flag == "GLOBAL")
                AddPropertyElement(xmlDoc, objectElement2, "isGlobal", "true");

            // Check if ReadOnly
            if (flags.Contains("READONLY") || flag == "READONLY")
                AddPropertyElement(xmlDoc, objectElement2, "isReadOnly", "true");

            // If Discrete, then setup up var for thing below
            if (flags.Contains("DISCRETE") || flag == "DISCRETE")
                parmType = 1;

            // If Labeled, then setup up var for thing below
            if (flags.Contains("LABELED") || flag == "LABELED")
                parmType = 2;
        }

        // "0" == Continuious (is default and missing in XML)
        // "1" == Discrete
        // "2" == Labeled (requires some more stuff)
        if (parmType != 0)// If Discrete or Labeled
            AddPropertyElement(xmlDoc, objectElement2, "parameterType", parmType.ToString());
        if (parmType == 2)// If Labeled
        {
            // add labels
            var propElement = xmlDoc.CreateElement("property");
            propElement.SetAttribute("name", "enumerationLabels");

            // Add Label Names of all values
            for (var i = 0; i < parameter.maximum + 1; i++)
            {
                // Get Label Name
                evDesc.getParameterLabelByID(parameter.id, i, out string labelName);

                // Add to XML
                var labelElement = xmlDoc.CreateElement("value");
                labelElement.InnerText = labelName;
                propElement.AppendChild(labelElement);
            }

            objectElement2.AppendChild(propElement);
        }

        // idk what this is
        //AddPropertyElement(xmlDoc, objectElement2, "isExposedRecursively", "false");

        // probably not gonna be added
        //AddPropertyElement(xmlDoc, objectElement2, "seekSpeed", "1");
        //AddPropertyElement(xmlDoc, objectElement2, "seekSpeedDescending", "2");
        #endregion

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + "/Metadata/ParameterPreset/" + $"{{{XMLGUID}}}" + ".xml";

        // Save
        SaveXML(xmlDoc, filePath);
    }
}