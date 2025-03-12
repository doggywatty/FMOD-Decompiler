using System.Xml;
using static Program;

public class EventFolder
{
    // used in main
    public static List<string> AllEvents = new List<string> { };

    public static void ExtractEventFolders(string filePath, bool verbose) 
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
                    CreateXmlFile(filePath, folder, folders, path, verbose);

                    // Mark this folder as processed
                    processedFolders.Add(folder);
                }
            }
        }
    }
    static void CreateXmlFile(string filePath, string folderName, List<string> folders, string fullpath, bool verbose)
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

        // find how many subfolders down the folder is
        if (folders[0] == folderName) // if root event folder (like /music/)
            destinationElement.InnerText = $"{{{MasterEventFolderGUID}}}";
        // else if underneath another folder (like /music/soundtest/ or /music/soundtest/bgmusic/)
        else
        {
            var i = 1; // set to one to skip a useless loop
            while (true)
            {
                // find position 
                if (folders[i] == folderName)
                {
                    // make sure GUID doesn't reference itself
                    if (EventFolderGUIDs[folders[folders.Count - 1 - i]] != EventFolderGUIDs[folderName])
                    {
                        destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[folders.Count - 1 - i]]}}}";
                        break;
                    }
                    // but if it does, set to Master and notify user
                    else if (EventFolderGUIDs[folders[folders.Count - 1 - i]] == EventFolderGUIDs[folderName])
                    {
                        OrganizeError = true;
                        if (verbose)
                        {
                            Console.WriteLine($"{RED}Event Folder Missmatch!{NORMAL}");
                            Console.WriteLine($"{RED}Event Folder \"{folderName}\" from event \"{fullpath}\"{NORMAL}");
                            if (SafeOrgLevel == 1)
                                Console.WriteLine($"{RED}Trying Experimental Folder Placement...{NORMAL}");
                            else
                                Console.WriteLine($"{RED}Setting Folder to Master as fallback...{NORMAL}");
                        }
                        // SafeOrgLevel is never 2 here
                        if (SafeOrgLevel == 1)
                            destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[folders.Count - 2 - i]]}}}";
                        else
                            destinationElement.InnerText = $"{{{MasterEventFolderGUID}}}";
                        break;
                    }
                }

                // if none, add to check and repeat the loop
                i++;
            }
        }

        relationshipElement.AppendChild(destinationElement);
        objectElement.AppendChild(relationshipElement);

        // Save the XML document to File
        xmlDoc.Save(filePath + $"/{{{EventFolderGUIDs[folderName]}}}.xml");
    }
}
