using System.Xml;
using static Program;
using static XMLHelper;

public class EventFolder
{
	#region Find All Event Folders
	public static void ExtractEventFolders(string EventPath) 
	{
		// figure out all subfolders from an event path and make an XML for each subfolder
		// aka event:/music/soundtest/pause
		//			^folder ^folder  ^event (ignore event)

		List<string> folders = SplitEventPath(EventPath);

		// goes through every subfolder in cleansed path (ex: /music and /soundtest)
		int folder_level = 0;
		foreach (var folder in folders)
		{
			if (!EventFolderGUIDs.ContainsKey(folder + $"{folder_level}"))
			{
				PushToConsoleLog($"Saving Event Folder: /{folder}", MAGENTA);

				// Create GUID for "folder"
				EventFolderGUIDs.TryAdd(folder + $"{folder_level}", GetRandomGUID());

				// Create XML
				EventFolderXML(folder, folders, folder_level);
			}
			// increase after every folder
			folder_level++;
		}
	}
	#endregion
	#region Event Folder XML
	static void EventFolderXML(string folderName, List<string> folders, int folder_level)
	{
		Guid eFolderGuid = EventFolderGUIDs[folderName + $"{folder_level}"];

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		// Create Header and Link its own GUID to itself
		SetupHeaderXML(xmlDoc, root, "EventFolder", $"{{{eFolderGuid}}}", out XmlElement objectElement);

		// Set its Folder Name
		AddPropertyElement(xmlDoc, objectElement, "name", folderName);

		// Get GUID of Higher Folder
		string linkGUID = $"{{{MasterEventFolderGUID}}}";// Default to MasterEventFolder

		// check if current folder isn't a root event folder (like event:/music/)
		// because those have to use default
		if (folders[0] != folderName)
		{
			// Get GUID of Higher Folder
			int higher_folder = folder_level - 1;
			// Link it
			linkGUID = $"{{{EventFolderGUIDs[folders[higher_folder] + $"{higher_folder}"]}}}";
		}
		// Link the GUID of the folder above it
		AddRelationshipElement(xmlDoc, objectElement, "folder", linkGUID);

		// Save the XML document to File
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/EventFolder/{{{eFolderGuid}}}.xml");
	}
	#endregion
}
