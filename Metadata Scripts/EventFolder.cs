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
                    if (verbose && SafeOrgLevel < 2)
                    {
                        Console.WriteLine($"{MAGENTA}Saving Event Folder: \\{folder}{NORMAL}");
                        Console.WriteLine($"{MAGENTA}    From : \"{path}\"{NORMAL}");
                    }

                    // Create GUID for "folder"
                    EventFolderGUIDs.TryAdd(folder + $"{folder_level}", GetRandomGUID());

                    // Create XML
                    CreateXmlFile(filePath, folder, folders, folder_level, path, verbose);

                    // Mark this folder as processed
                    processedFolders.Add(folder + $"{folder_level}");
                }
                // increase after every folder
                folder_level++;
            }
        }
    }
    static void CreateXmlFile(string filePath, string folderName, List<string> folders, int folder_level, string fullpath, bool verbose)
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
            var i = 1; // set to one to skip a useless loop
            while (true)
            {
                // find position 
                if (folders[i] == folderName)
                {
                    // bro EVEN I DONT KNOW WHAT I DID
                    // this crackhead code might have to be revised the more subfolders there are, but idk
                    int find_folder_level = folder_level > folders.Count - 2 ? folder_level - 1 : folder_level;

                    // make sure GUID doesn't reference itself
                    if (EventFolderGUIDs[folders[folders.Count - 2] + $"{find_folder_level}"] != EventFolderGUIDs[folderName + $"{folder_level}"])
                    {
                        destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[folders.Count - 2] + $"{find_folder_level}"]}}}";
                        break;
                    }
                    // but if it does, set to something else and notify user
                    else if (EventFolderGUIDs[folders[folders.Count - 2] + $"{find_folder_level}"] == EventFolderGUIDs[folderName + $"{folder_level}"])
                    {
                        OrganizeError = true;
                        if (verbose)
                        {
                            Console.WriteLine($"{RED}Event Folder Missmatch!{NORMAL}");
                            Console.WriteLine($"{RED}Event Folder \"{folderName}\" from event \"{fullpath}\"{NORMAL}");
                            if (SafeOrgLevel == 1)
                                Console.WriteLine($"{GREEN}Trying Experimental Folder Placement...{NORMAL}");
                            else
                                Console.WriteLine($"{RED}Setting Folder to Master as fallback...{NORMAL}");
                        }
                        // SafeOrgLevel is never 2 here
                        if (SafeOrgLevel == 1) // if experimental, send it to the higher folder
                            destinationElement.InnerText = $"{{{EventFolderGUIDs[folders[folders.Count - 3] + $"{find_folder_level - 1}"]}}}";
                        else // else just set it to Master and call it a day
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
        xmlDoc.Save(filePath + $"/{{{EventFolderGUIDs[folderName + $"{folder_level}"]}}}.xml");
    }
}
