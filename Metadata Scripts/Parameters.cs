using System.ComponentModel;
using System.Xml;
using FMOD.Studio;
using static Program;

public class Parameters
{
    public static List<PARAMETER_DESCRIPTION> ParameterList = new List<PARAMETER_DESCRIPTION>();
    public static void ParmeterXML(PARAMETER_DESCRIPTION parameter, EventDescription evDesc)
    {
        Guid XMLGUID = FMODGUIDToSysGuid(parameter.guid);
        Guid ParameterSettings = GetRandomGUID();

        // string name of the parameter
        string parmName = parameter.name;

        // Setup XML
        var xmlDoc = new XmlDocument();
        var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDeclaration);
        var root = xmlDoc.CreateElement("objects");
        root.SetAttribute("serializationModel", "Studio.02.02.00");

        #region First Segment
        var objectElement1 = xmlDoc.CreateElement("object");
        objectElement1.SetAttribute("class", "ParameterPreset");
        objectElement1.SetAttribute("id", $"{{{XMLGUID}}}");
        root.AppendChild(objectElement1);

        // Parmeter Name
        AddPropertyElement(xmlDoc, objectElement1, "name", parmName);
        // Link back to Master Parm GUID
        AddRelationshipElement(xmlDoc, objectElement1, "folder", $"{{{MasterParameterPresetGUID}}}");
        // Link to Parameter Settings in Second Segment below
        AddRelationshipElement(xmlDoc, objectElement1, "parameter", $"{{{ParameterSettings}}}");
        #endregion

        #region Second Segment
        var objectElement2 = xmlDoc.CreateElement("object");
        objectElement2.SetAttribute("class", "GameParameter");
        objectElement2.SetAttribute("id", $"{{{ParameterSettings}}}");
        root.AppendChild(objectElement2);

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

        // Check if isGlobal | 0x00000004
        var flags = (uint)parameter.flags;
        if (flags == (uint)PARAMETER_FLAGS.GLOBAL)
            AddPropertyElement(xmlDoc, objectElement2, "isGlobal", "true");

        // Check if ReadOnly | 0x00000001
        if (flags == (uint)PARAMETER_FLAGS.READONLY)
            AddPropertyElement(xmlDoc, objectElement2, "isReadOnly", "true");

        // Since this property takes only integers
        // cast enum as int first so we don't get enum name
        // "0" == Continuious (is missing in XML)
        // "1" == Discrete
        // "2" == Labeled (requires some more stuff)
        int parmType = (int)parameter.type;
        if (parmType != 0)// If Discrete or Labeled
            AddPropertyElement(xmlDoc, objectElement2, "parameterType", parmType.ToString());
        else if (parmType == 2)// If Labeled
        {
            // add labels
            var propElement = xmlDoc.CreateElement("property");
            propElement.SetAttribute("name", "enumerationLabels");

            // Add Label Names of all values
            for (var i = 0; i < parameter.maximum; i++)
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

        //AddPropertyElement(xmlDoc, objectElement2, "isReadOnly", "true");

        // probably not gonna be added
        //AddPropertyElement(xmlDoc, objectElement2, "seekSpeed", "1");
        //AddPropertyElement(xmlDoc, objectElement2, "seekSpeedDescending", "2");
        #endregion

        xmlDoc.AppendChild(root);

        // XML File Path
        string filePath = outputProjectPath + "/Metadata/ParameterPreset/" + $"{{{XMLGUID}}}" + ".xml";

        // Save
        xmlDoc.Save(filePath);
    }
    private static void AddPropertyElement(XmlDocument xmlDoc, XmlElement parent, string propertyName, string value)
    {
        var propertyElement = xmlDoc.CreateElement("property");
        propertyElement.SetAttribute("name", propertyName);

        var valueElement = xmlDoc.CreateElement("value");
        valueElement.InnerText = value;

        propertyElement.AppendChild(valueElement);
        parent.AppendChild(propertyElement);
    }
    private static void AddRelationshipElement(XmlDocument xmlDoc, XmlElement parent, string name, string value)
    {
        var propertyElement = xmlDoc.CreateElement("relationship");
        propertyElement.SetAttribute("name", name);

        var distinationElement = xmlDoc.CreateElement("destination");
        distinationElement.InnerText = value;

        propertyElement.AppendChild(distinationElement);
        parent.AppendChild(propertyElement);
    }
}