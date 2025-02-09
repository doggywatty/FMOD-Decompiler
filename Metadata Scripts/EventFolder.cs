using System.Xml;
using static Program;

public class EventFolder
{
    // used in main
    public static List<string> AllEvents = new List<string> { };

    public static void ExtractEventFolders(string filePath) 
    {
        // figure out all subfolders from an event path and make an XML for each subfolder
        // aka event:/music/soundtest/pause
        //            ^folder ^folder  ^event (ignore event)

        // This HashSet will track which folders have already been processed
        HashSet<string> processedFolders = new HashSet<string>();

        foreach (var path in AllEvents)
        {
            // Split the path by "/"
            var pathParts = path.Split('/');

            // Get the subfolders
            var folders = new List<string>(pathParts);
            folders.RemoveAt(0); // Remove "event:"
            folders.RemoveAt(folders.Count - 1); // Remove the last part (it's not a folder)

            // goes through every subfolder in cleansed path (ex: /music and /soundtest)
            foreach (var folder in folders)
            {
                if (!processedFolders.Contains(folder))
                {
                    // Create GUID for "folder"
                    EventFolderGUIDs.TryAdd(folder, GetRandomGUID());

                    // Create XML
                    CreateXmlFile(filePath, folder, folders);

                    // Mark this folder as processed
                    processedFolders.Add(folder);
                }
            }
        }
    }
    // Honestly I should have done this for AudioFile.cs ngl
    static void CreateXmlFile(string filePath, string folderName, List<string> folders)
    {
        // Create XML document
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declaration);

        // Create the root element
        XmlElement root = xmlDoc.CreateElement("objects");
        root.SetAttribute("serializationModel", "Studio.02.02.00");
        xmlDoc.AppendChild(root);

        // Create object element
        XmlElement objectElement = xmlDoc.CreateElement("object");
        objectElement.SetAttribute("class", "EventFolder");
        objectElement.SetAttribute("id", $"{{{EventFolderGUIDs[folderName]}}}");
        root.AppendChild(objectElement);

        // Create property element for the name
        XmlElement propertyElement = xmlDoc.CreateElement("property");
        propertyElement.SetAttribute("name", "name");
        XmlElement valueElement = xmlDoc.CreateElement("value");
        valueElement.InnerText = folderName;
        propertyElement.AppendChild(valueElement);
        objectElement.AppendChild(propertyElement);

        // GUID of the folder above it
        XmlElement relationshipElement = xmlDoc.CreateElement("relationship");
        relationshipElement.SetAttribute("name", "folder");
        XmlElement destinationElement = xmlDoc.CreateElement("destination");

        // if event is in a folder, and isn't located in root
        // aka you had this: event:/music/soundtest/pause
        // and now its down to : /music/soundtest/
        if (folders.Count >= 2)
            destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[folders.Count - 2]]}}}"; // from the example, you get /music's GUID
        else if (folders.Count == 1)
            destinationElement.InnerText = $"{{{MasterEventFolderGUID}}}";
        // there should definitely not be 0

        relationshipElement.AppendChild(destinationElement);
        objectElement.AppendChild(relationshipElement);

        // Save the XML document to File
        xmlDoc.Save(filePath + $"/{{{EventFolderGUIDs[folderName]}}}.xml");
    }
}
