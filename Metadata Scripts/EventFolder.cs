using System.Xml;
using static Program;

public class EventFolder
{
    // used in main
    public static List<string> AllEvents = new List<string> { };

    // This HashSet will track which folders have already been processed
    public static HashSet<string> processedFolders = new HashSet<string>();

    public static void ExtractEventFolders(string filePath, bool verbose) 
    {
        // figure out all subfolders from an event path and make an XML for each subfolder
        // aka event:/music/soundtest/pause
        //            ^folder ^folder  ^event (ignore event)

        foreach (var path in AllEvents)
        {
            // Split the path by "/"
            var pathParts = path.Split('/');

            // Get the subfolders
            var folders = new List<string>(pathParts);
            folders.RemoveAt(0); // Remove "event:"
            folders.RemoveAt(folders.Count - 1); // Remove the last part (it's not a folder)

            // goes through every subfolder in cleansed path (ex: /music and /soundtest)
            int folder_level = 0;
            foreach (var folder in folders)
            {
                if (!processedFolders.Contains(folder + $"{folder_level}"))
                {
                    if (verbose && !No_Org)
                        Console.WriteLine($"{MAGENTA}Saving Event Folder: /{folder}{NORMAL}");

                    // Create GUID for "folder"
                    EventFolderGUIDs.TryAdd(folder + $"{folder_level}", GetRandomGUID());

                    // Create XML
                    CreateXmlFile(filePath, folder, folders, folder_level);

                    // Mark this folder as processed
                    processedFolders.Add(folder + $"{folder_level}");
                }
                // increase after every folder
                folder_level++;
            }
        }
    }
    static void CreateXmlFile(string filePath, string folderName, List<string> folders, int folder_level)
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
        objectElement.SetAttribute("id", $"{{{EventFolderGUIDs[folderName + $"{folder_level}"]}}}");
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

        // find how many subfolders down the folder is
        if (folders[0] == folderName) // if root event folder (like /music/)
            destinationElement.InnerText = $"{{{MasterEventFolderGUID}}}";
        // else if underneath another folder (like /music/soundtest/ or /music/soundtest/bgmusic/)
        else
        {
            // check position 
            var higher_folder = folder_level - 1;
            if (folders[folder_level] == folderName)//get guid of higher folder 
                destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[higher_folder] + $"{higher_folder}"]}}}";
            else // shouldn't happen, but just in case
                destinationElement.InnerText = $"{{{MasterEventFolderGUID}}}";
        }

        relationshipElement.AppendChild(destinationElement);
        objectElement.AppendChild(relationshipElement);

        // Save the XML document to File
        xmlDoc.Save(filePath + $"/{{{EventFolderGUIDs[folderName + $"{folder_level}"]}}}.xml");
    }
}
