using System.Security.Claims;
using System.Xml;
using static Program;
using static XMLHelper;

public class EventFolder
{
    // used in main
    public static List<string> AllEvents = new List<string> { };

    // This HashSet will track which folders have already been processed
    public static HashSet<string> processedFolders = new HashSet<string>();

    public static void ExtractEventFolders(string filePath) 
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
                    PushToConsoleLog($"Saving Event Folder: /{folder}", MAGENTA);

                    // Create GUID for "folder"
                    EventFolderGUIDs.TryAdd(folder + $"{folder_level}", GetRandomGUID());

                    // Create XML
                    EventFolderXML(filePath, folder, folders, folder_level);

                    // Mark this folder as processed
                    processedFolders.Add(folder + $"{folder_level}");
                }
                // increase after every folder
                folder_level++;
            }
        }
    }
    static void EventFolderXML(string directorypath, string folderName, List<string> folders, int folder_level)
    {
        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
        xmlDoc.AppendChild(root);

        // Create Header and Link its own GUID to itself
        SetupHeaderXML(xmlDoc, root, "EventFolder", $"{{{EventFolderGUIDs[folderName + $"{folder_level}"]}}}", out XmlElement objectElement);

        // Set its Folder Name
        AddPropertyElement(xmlDoc, objectElement, "name", folderName);

        // Link the GUID of the folder above it
        var relationshipElement = xmlDoc.CreateElement("relationship");
        relationshipElement.SetAttribute("name", "folder");
        var destinationElement = xmlDoc.CreateElement("destination");

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

        string filePath = directorypath + $"/{{{EventFolderGUIDs[folderName + $"{folder_level}"]}}}.xml";

        // Save the XML document to File
        SaveXML(xmlDoc, filePath);
    }
}
