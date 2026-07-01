using System.Xml;
using static Program;
using static XMLHelper;

public class EventFolder
{
	#region Find All Event Folders
	public static void ExtractEventFolders(string EventPath) 
	{
		List<string> folders = SplitEventPath(EventPath);

		int folder_level = 0;
		foreach (var folder in folders)
		{
			string folderKey = string.Join("/", folders.Take(folder_level + 1));

			if (!EventFolderGUIDs.ContainsKey(folderKey))
			{
				PushToConsoleLog($"Saving Event Folder: /{folderKey}", MAGENTA);
				EventFolderGUIDs.TryAdd(folderKey, GetRandomGUID());
				EventFolderXML(folderKey, folders, folder_level);
			}
			folder_level++;
		}
	}
	#endregion
	#region Event Folder XML
	static void EventFolderXML(string folderKey, List<string> folders, int folder_level)
	{
		Guid eFolderGuid = EventFolderGUIDs[folderKey];

		// Setup XML
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		xmlDoc.AppendChild(root);

		// Create Header and Link its own GUID to itself
		SetupHeaderXML(xmlDoc, root, "EventFolder", $"{{{eFolderGuid}}}", out XmlElement objectElement);

		AddPropertyElement(xmlDoc, objectElement, "name", folders[folder_level]);

		// Determine parent folder key (full path of the folder above)
		string linkGUID = $"{{{MasterEventFolderGUID}}}";
		if (folder_level > 0)
		{
			string parentKey = string.Join("/", folders.Take(folder_level));
			linkGUID = $"{{{EventFolderGUIDs[parentKey]}}}";
		}
		AddRelationshipElement(xmlDoc, objectElement, "folder", linkGUID);

		// Save the XML document to File
		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/EventFolder/{{{eFolderGuid}}}.xml");
	}
	#endregion
}
