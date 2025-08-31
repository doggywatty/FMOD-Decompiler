// Extract Event Info and Create Event XML
using System.Xml;
using static Program;
using static XMLHelper;
#pragma warning disable CS8602
public class Events
{
    public static void SaveEvents(string eventname, string bankfilename, List<EventSoundInfo> SoundsinEvent, bool IsAction = false)
    {
        // these change per XML, but not within the XML
        // so they should be here, and not public static
        Guid EventMixerGuid = GetRandomGUID();
        Guid MasterTrackGuid = GetRandomGUID();
        Guid MixerInputGuid = GetRandomGUID();
        Guid EventAutomatablePropertiesGuid = GetRandomGUID();
        Guid MarkerTrackGuid = GetRandomGUID();
        Guid TimelineGuid = GetRandomGUID();
        Guid EventMixerMasterGuid = GetRandomGUID();
        Guid MixerBusEffectChainGuid1 = GetRandomGUID();
        Guid MixerBusEffectChainGuid2 = GetRandomGUID();
        Guid MixerBusPannerGuid1 = GetRandomGUID();
        Guid MixerBusPannerGuid2 = GetRandomGUID();
        Guid MixerBusFaderGuid1 = GetRandomGUID();
        Guid MixerBusFaderGuid2 = GetRandomGUID();

        // if audiofile on timeline, assign internal GUID
        var SoundsPresent = SoundsinEvent != null && SoundsinEvent.Count != 0;
        Guid[] multisoundGUIDs = [];
        if (SoundsPresent)
        {
            multisoundGUIDs = new Guid[SoundsinEvent.Count];
            var i = 0;
            foreach (var sound in SoundsinEvent)
            {
                multisoundGUIDs[i] = GetRandomGUID();
                i++;
            }
        }
        // Only use when Action
        Guid MultiSoundGuid = GetRandomGUID();
        Guid ActionSheetGuid = GetRandomGUID();

        // Setup XML
        SetupXML(out XmlDocument xmlDoc, out XmlElement root);
        xmlDoc.AppendChild(root);

        #region Main Event Info Links
        SetupHeaderXML(xmlDoc, root, "Event", $"{{{EventGUIDs[eventname]}}}", out XmlElement EventElement);
        //                                               get shortened event name out of event path
        AddPropertyElement(xmlDoc, EventElement, "name", GetName(eventname));
        AddPropertyElement(xmlDoc, EventElement, "outputFormat", "0");
        //                                                          get name of folder containing the event
        AddRelationshipElement(xmlDoc, EventElement, "folder", $"{{{GetHigherEventFolder(eventname)}}}");
        AddRelationshipElement(xmlDoc, EventElement, "mixer", $"{{{EventMixerGuid}}}");
        AddRelationshipElement(xmlDoc, EventElement, "masterTrack", $"{{{MasterTrackGuid}}}");
        AddRelationshipElement(xmlDoc, EventElement, "mixerInput", $"{{{MixerInputGuid}}}");
        AddRelationshipElement(xmlDoc, EventElement, "automatableProperties", $"{{{EventAutomatablePropertiesGuid}}}");
        AddRelationshipElement(xmlDoc, EventElement, "markerTracks", $"{{{MarkerTrackGuid}}}");
        AddRelationshipElement(xmlDoc, EventElement, "timeline", $"{{{TimelineGuid}}}");
        // Add Action Sheet if it is one
        if (SoundsPresent && IsAction)
            AddRelationshipElement(xmlDoc, EventElement, "parameters", $"{{{ActionSheetGuid}}}");
        //                                                          connects event to its original bank file
        AddRelationshipElement(xmlDoc, EventElement, "banks", $"{{{BankSpecificGUIDs[bankfilename + "_Bank"]}}}");
        #endregion

        #region Event Info
        SetupHeaderXML(xmlDoc, root, "EventMixer", $"{{{EventMixerGuid}}}", out XmlElement EventMixerElement);
        AddRelationshipElement(xmlDoc, EventMixerElement, "masterBus", $"{{{EventMixerMasterGuid}}}");

        SetupHeaderXML(xmlDoc, root, "MasterTrack", $"{{{MasterTrackGuid}}}", out XmlElement MasterTrackElement);
        // if audiofile on timeline (and isn't Action)
        if (SoundsPresent && !IsAction)
            AddMultiRelationshipElement(xmlDoc, MasterTrackElement, "modules", multisoundGUIDs);
        // else if sounds and is Action
        else if (SoundsPresent && IsAction)
            AddRelationshipElement(xmlDoc, MasterTrackElement, "modules", $"{{{MultiSoundGuid}}}");

        AddRelationshipElement(xmlDoc, MasterTrackElement, "mixerGroup", $"{{{EventMixerMasterGuid}}}");

        SetupHeaderXML(xmlDoc, root, "MixerInput", $"{{{MixerInputGuid}}}", out XmlElement MixerInputElement);
        AddRelationshipElement(xmlDoc, MixerInputElement, "effectChain", $"{{{MixerBusEffectChainGuid1}}}");
        AddRelationshipElement(xmlDoc, MixerInputElement, "panner", $"{{{MixerBusPannerGuid1}}}");
        AddRelationshipElement(xmlDoc, MixerInputElement, "output", $"{{{MasterXMLGUID}}}"); // connected to Master.xml

        // Empty Headers (for now maybe)
        SetupHeaderXML(xmlDoc, root, "EventAutomatableProperties", $"{{{EventAutomatablePropertiesGuid}}}", out XmlElement EventAutomatablePropertiesElement);
        SetupHeaderXML(xmlDoc, root, "MarkerTrack", $"{{{MarkerTrackGuid}}}", out XmlElement MarkerTrackElement);

        // Empty header if no sounds are present
        SetupHeaderXML(xmlDoc, root, "Timeline", $"{{{TimelineGuid}}}", out XmlElement TimelineElement);
        if (SoundsPresent && !IsAction)
            AddMultiRelationshipElement(xmlDoc, TimelineElement, "modules", multisoundGUIDs);
        else if (SoundsPresent && IsAction)
            AddPropertyElement(xmlDoc, TimelineElement, "isProxyEnabled", "false");

        // Only for Action Sheet
        if (SoundsPresent && IsAction) 
        {
            SetupHeaderXML(xmlDoc, root, "ActionSheet", $"{{{ActionSheetGuid}}}", out XmlElement ActionSheetElement);
            AddRelationshipElement(xmlDoc, ActionSheetElement, "modules", $"{{{MultiSoundGuid}}}");
        }

        SetupHeaderXML(xmlDoc, root, "EventMixerMaster", $"{{{EventMixerMasterGuid}}}", out XmlElement EventMixerMasterElement);
        AddRelationshipElement(xmlDoc, EventMixerMasterElement, "effectChain", $"{{{MixerBusEffectChainGuid2}}}");
        AddRelationshipElement(xmlDoc, EventMixerMasterElement, "panner", $"{{{MixerBusPannerGuid2}}}");
        AddRelationshipElement(xmlDoc, EventMixerMasterElement, "mixer", $"{{{EventMixerGuid}}}");

        // Only for Action Sheet
        if (SoundsPresent && IsAction)
        {
            SetupHeaderXML(xmlDoc, root, "MultiSound", $"{{{MultiSoundGuid}}}", out XmlElement MultiSoundElement);
            AddMultiRelationshipElement(xmlDoc, MultiSoundElement, "sounds", multisoundGUIDs);
        }

        // Here is where Single Sounds would go
        if (SoundsPresent)
        {
            var i = 0;
            foreach (var sound in SoundsinEvent)
            {
                SetupHeaderXML(xmlDoc, root, "SingleSound", $"{{{multisoundGUIDs[i]}}}", out XmlElement SoundElement);
                // where the sound starts on the timeline (in seconds)
                if (sound.startpos != 0)
                    AddPropertyElement(xmlDoc, SoundElement, "start", $"{sound.startpos}");
                // length in milliseconds
                AddPropertyElement(xmlDoc, SoundElement, "length", $"{sound.length}");

                /*
                 * sound.getLoopCount doesn't work as intended
                 * so we need another solution

                // if loopcount == -1 or 1+
                if (sound.loopcount == -1 || sound.loopcount > 0)
                    AddPropertyElement(xmlDoc, SoundElement, "looping", $"true");
                // if loopcount is 1+ (not -1 or 0)
                if (sound.loopcount > 0)
                    AddPropertyElement(xmlDoc, SoundElement, "playCount", $"{sound.loopcount}");
                */

                // link audiofile GUID (always there)
                AddRelationshipElement(xmlDoc, SoundElement, "audioFile", $"{{{sound.GUID}}}");
                i++;
            }
        }

        SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid1}}}", out XmlElement MixerBusEffectChainElement1);
        AddRelationshipElement(xmlDoc, MixerBusEffectChainElement1, "effects", $"{{{MixerBusFaderGuid1}}}");

        // Empty for now
        SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid1}}}", out XmlElement MixerBusPannerElement1);

        SetupHeaderXML(xmlDoc, root, "MixerBusEffectChain", $"{{{MixerBusEffectChainGuid2}}}", out XmlElement MixerBusEffectChainElement2);
        AddRelationshipElement(xmlDoc, MixerBusEffectChainElement2, "effects", $"{{{MixerBusFaderGuid2}}}");

        // Empty Headers (for now maybe)
        SetupHeaderXML(xmlDoc, root, "MixerBusPanner", $"{{{MixerBusPannerGuid2}}}", out XmlElement MixerBusPannerElement2);
        SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusFaderGuid1}}}", out XmlElement MixerBusFaderElement1);
        SetupHeaderXML(xmlDoc, root, "MixerBusFader", $"{{{MixerBusPannerGuid2}}}", out XmlElement MixerBusFaderElement2);
        #endregion

        // Output Filepath
        string filePath = outputProjectPath + $"/Metadata/Event/{{{EventGUIDs[eventname]}}}.xml";

        // Save the XML document to a file
        SaveXML(xmlDoc, filePath);
    }

    #region Get Event Names
    // Get Folder above
    public static string GetHigherEventFolder(string eventname)
    {
        // Split the path by "/"
        var pathParts = eventname.Split('/');

        // Get the subfolders
        var folders = new List<string>(pathParts);
        folders.RemoveAt(0); // Remove "event:"
        folders.RemoveAt(folders.Count - 1); // Remove the last part (it's not a folder)

        // if like event:/music/soundtest/pause, or event:/soundtest/pause, get /soundtest
        if (folders.Count >= 1)
            return $"{EventFolderGUIDs[folders[folders.Count - 1] + $"{folders.Count - 1}"]}";
        // else if like event:/sound, get Master Folder
        else
            return $"{MasterEventFolderGUID}";
    }

    // Get Shortened Name
    public static string GetName(string eventname)
    {
        // Split the path by "/"
        var pathParts = eventname.Split('/');

        // Get the last part (event name)
        var folders = new List<string>(pathParts);
        return $"{folders[folders.Count - 1]}";
    }
    #endregion
}
