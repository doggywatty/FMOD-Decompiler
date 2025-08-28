using System.Xml;
using FMOD.Studio;
using static Program;

public class Parameters
{
    public static List<PARAMETER_DESCRIPTION> ParameterList = new List<PARAMETER_DESCRIPTION>();
    public static void ParmeterXML(PARAMETER_DESCRIPTION parameter)
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
        AddPropertyElement(xmlDoc, objectElement2, "initialValue", parameter.defaultvalue.ToString());
        AddPropertyElement(xmlDoc, objectElement2, "minimum", parameter.minimum.ToString());
        AddPropertyElement(xmlDoc, objectElement2, "maximum", parameter.maximum.ToString());

        // Check if isGlobal
        if (parameter.flags == PARAMETER_FLAGS.GLOBAL)
            AddPropertyElement(xmlDoc, objectElement2, "isGlobal", "true");//can be missing

        // Since this property takes only integers
        // cast enum as int first so we don't get enum name
        int parmType = (int)parameter.type;
        AddPropertyElement(xmlDoc, objectElement2, "parameterType", parmType.ToString());

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