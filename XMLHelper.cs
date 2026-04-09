using System.Xml;

public class XMLHelper
{
	// Declarations that is present in basically every XML file in the FMOD Project
	#region Setup Functions
	public static void SetupXML(out XmlDocument xmlDoc, out XmlElement root)
	{
		xmlDoc = new XmlDocument();
		var declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
		xmlDoc.AppendChild(declaration);
		root = xmlDoc.CreateElement("objects");
		root.SetAttribute("serializationModel", "Studio.02.02.00");
	}
	public static void SetupHeaderXML(XmlDocument xmlDoc, XmlElement root, string Class, string ID, out XmlElement objectElement)
	{
		objectElement = xmlDoc.CreateElement("object");
		objectElement.SetAttribute("class", Class);
		objectElement.SetAttribute("id", ID);
		root.AppendChild(objectElement);
	}
	#endregion

	#region Create Element Functions
	// Used to set Properties on an object
	public static void AddPropertyElement(XmlDocument xmlDoc, XmlElement root, string name, string value)
	{
		var propertyElement = xmlDoc.CreateElement("property");
		propertyElement.SetAttribute("name", name);

		var valueElement = xmlDoc.CreateElement("value");
		valueElement.InnerText = value;

		propertyElement.AppendChild(valueElement);
		root.AppendChild(propertyElement);
	}

	public static void AddMultiPropertyElement(XmlDocument xmlDoc, XmlElement root, string name, string[] values)
	{
		var propertyElement = xmlDoc.CreateElement("property");
		propertyElement.SetAttribute("name", name);
		foreach (string value in values)
		{
			var valueElement = xmlDoc.CreateElement("value");
			valueElement.InnerText = value;
			propertyElement.AppendChild(valueElement);
		}

		root.AppendChild(propertyElement);
	}

	// Used to link back to another XML File using its GUID
	// Or to link to a section of the XML File using a GUID
	public static void AddRelationshipElement(XmlDocument xmlDoc, XmlElement root, string name, string value)
	{
		var relationshipElement = xmlDoc.CreateElement("relationship");
		relationshipElement.SetAttribute("name", name);

		var distinationElement = xmlDoc.CreateElement("destination");
		distinationElement.InnerText = value;

		relationshipElement.AppendChild(distinationElement);
		root.AppendChild(relationshipElement);
	}

	public static void AddMultiRelationshipElement(XmlDocument xmlDoc, XmlElement root, string name, Guid[] values)
	{
		var relationshipElement = xmlDoc.CreateElement("relationship");
		relationshipElement.SetAttribute("name", name);
		foreach (Guid value in values)
		{
			var distinationElement = xmlDoc.CreateElement("destination");
			distinationElement.InnerText = $"{{{value}}}";
			relationshipElement.AppendChild(distinationElement);
		}

		root.AppendChild(relationshipElement);
	}
	#endregion

	#region Save XML Function
	// Save the XML File with Indentations and stuff
	public static void SaveXML(XmlDocument xmlDoc, string filepath)
	{
		// Make Indentation Settings
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "\t",
			NewLineOnAttributes = false // avoid new lines
		};

		// Save the XML document to a file
		using (XmlWriter writer = XmlWriter.Create(filepath, settings))
			xmlDoc.WriteTo(writer);
	}
	#endregion
}
