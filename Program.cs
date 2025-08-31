using FMOD.Studio;
using System.Runtime.InteropServices;
public class Program
{
    #region Compiler Warning bullshit

    #pragma warning disable CS1998
    #pragma warning disable CS4014
    #pragma warning disable CS8600
    #pragma warning disable CS8601
    #pragma warning disable CS8602
    #pragma warning disable CS8603
    #pragma warning disable CS8604
    #pragma warning disable CS8605
    #pragma warning disable CS8625

    #endregion

    #region Colored Text
    // thank you https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
    public static string SPACE = "\r                                            "; // shortcut for when not verbose
    public static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    public static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    public static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    public static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
    public static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
    public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    public static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
    public static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
    public static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
    public static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
    public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    public static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
    public static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
    public static string OTHERGRAY = Console.IsOutputRedirected ? "" : "\x1b[90m";
    public static string BROWN = Console.IsOutputRedirected ? "" : "\x1b[38;5;94m";

    // and thank you https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr handle, out int mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int handle);
    #endregion

    #region Static GUIDs
    // since they are static, it'll only run once, so they should stay the same
    public static Guid MasterAssetsGUID = GetRandomGUID();
    public static Guid MasterBankFolderGUID = GetRandomGUID();
    public static Guid MasterBankGUID = GetRandomGUID();
    public static Guid MasterEventFolderGUID = GetRandomGUID();
    public static Guid MasterPlatformGUID = GetRandomGUID();
    public static Guid MasterEncodingSettingGUID = GetRandomGUID();
    public static Guid MasterEffectPresetGUID = GetRandomGUID();
    public static Guid MasterParameterPresetGUID = GetRandomGUID();
    public static Guid MasterProfilerFolderGUID = GetRandomGUID();
    public static Guid MasterSandboxFolderGUID = GetRandomGUID();
    public static Guid MasterSnapshotGUID = GetRandomGUID();

    public static Guid MasterXMLGUID = GetRandomGUID();// for Master.XML
    public static Guid MasterMixerXMLGUID = GetRandomGUID();// for Mixer.XML
    public static Guid MasterTagsXMLGUID = GetRandomGUID();// for Tags.XML
    public static Guid MasterWorkspaceXMLGUID = GetRandomGUID();// for Workspace.XML

    public static Guid Master1GUID = GetRandomGUID();// for Master.XML (effectChain)
    public static Guid Master2GUID = GetRandomGUID();// for Master.XML (panner)
    public static Guid Master3GUID = GetRandomGUID();// for Master.XML (effect)

    // these keep track of all randomly generated GUIDs, so we can call them back if needed elsewhere
    public static Dictionary<string, Guid> EventGUIDs = [];
    public static Dictionary<string, Guid> EventFolderGUIDs = [];
    public static Dictionary<string, Guid> AudioFileGUIDs = [];
    public static Dictionary<string, Guid> BankSpecificGUIDs = [];
    #endregion

    #region Initialize Main Variables
    // For Spinner
    public static CancellationTokenSource SpinnerKill = new CancellationTokenSource();
    public static bool SpinnerInit = false;
    public static int SpinnerPattern = new Random().Next(2);

    // Argument Values
    public static string bankFolder = "";
    public static string outputProjectPath = "";
    public static bool verbose = false;
    #endregion

    #region Helper Funcs
    public static void PushToConsoleLog(string message, string color = "NONE", bool toLog = false)
    {
        // for some reason I can't just string color = NORMAL at the beginning because compiler cries
        var truecolor = (color == "NONE") ? NORMAL : color;

        // Whitelisted Strings (for when not in verbose)
        // i just really dont want to make another optional arg
        var ifwhitelisted = false;
        if (!verbose)
        {
            string[] whitelist = { "Loading Banks...", "Loaded Bank:", "Conversion Complete!", "Exported Project is at" };
            foreach (var str in whitelist)
            {
                if (message.Contains(str))
                {
                    ifwhitelisted = true;
                    break;
                }
            }
        }

        // Show Message on Console
        // Only if verbose is enabled, or if strings are in whitelist
        if (verbose || ifwhitelisted)
            Console.WriteLine($"{truecolor}{message}{NORMAL}");

        // If also saving to log
        if (toLog)
            File.AppendAllTextAsync(outputProjectPath + "/log.txt", "\n" + message);
 
    }
    // get random GUIDs for some stuff
    public static Guid GetRandomGUID()
    {
        // Generate a new GUID
        return Guid.NewGuid();
    }

    // FMOD.GUID to System.Guid Converter
    public static Guid FMODGUIDToSysGuid(FMOD.GUID fmodGuid)
    {
        // create byte array
        byte[] bytes = new byte[16];

        // copy FMOD.GUID Struct Data into the correct positions
        BitConverter.GetBytes(fmodGuid.Data1).CopyTo(bytes, 0);   // Data1 goes into positions 0-3
        BitConverter.GetBytes(fmodGuid.Data2).CopyTo(bytes, 4);   // Data2 goes into positions 4-7
        BitConverter.GetBytes(fmodGuid.Data3).CopyTo(bytes, 8);   // Data3 goes into positions 8-11
        BitConverter.GetBytes(fmodGuid.Data4).CopyTo(bytes, 12);  // Data4 goes into positions 12-15

        // return output as System.Guid
        return new Guid(bytes);
    }
    #endregion

    #region Event Structs
    // Struct for Sounds on Timeline
    public struct EventSoundInfo
    {
        public string name;
        public Guid GUID;
        public double startpos;
        public double length;
    }

    // Structs for Markers on Timeline
    public struct EventMarkerInfo
    {
        public string name;
        public double position;
    }

    // Structs for Parameters on Timeline
    public struct EventParameterInfo
    {
        public string name;
        public Guid GUID;
        public double value;
        public double start;
        public double length;
    }
    #endregion

    public static async Task Main(string[] args)
    {
        // initialize
        GetConsoleMode(GetStdHandle(-11), out int mode);
        SetConsoleMode(GetStdHandle(-11), mode | 0x4);
        Console.Clear();

        Console.WriteLine($"Welcome to the FMOD Bank Decompiler {GREEN}(Version 1.4.0){NORMAL}"
        + $"\n\nby {OTHERGRAY}burnedpopcorn180{NORMAL}"
        + $"\nand {BROWN}DogMatt{NORMAL}"

        + $"\n\n{RED}Note that this Decompiler tries its best to recreate the original project file{NORMAL}"
        + $"\n{RED}However, it won't give you a working recreation out of the box{NORMAL}"
        + $"\n{RED}You most likely will have to tweak things like events to get a functional recreation{NORMAL}"

        + $"\n{GREEN}With that being said, have fun{NORMAL}"

        + $"\n"
        );

        // Long ass shit that we should just ignore
        #region Arguments and Folders

        // check arguments
        for (int i = 0; i < args.Length; i++)
        {
            // check for --input argument
            if (args[i] == "--input" && i + 1 < args.Length)
            {
                bankFolder = args[i + 1];
                i++; // Skip the next element (value for --input)
            }
            // check the --output argument
            else if (args[i] == "--output" && i + 1 < args.Length)
            {
                outputProjectPath = args[i + 1];
                i++; // Skip the next element (value for --output)
            }
            // check the --verbose flag
            else if (args[i] == "--verbose")
            {
                verbose = true;
            }
            else
            {
                // Handle missing arguments
                Console.Write($"Missing argument: {args[i]}");
                return;
            }
        }

        // if no arguments were added
        if (args.Length == 0)
        {
            Console.Write("Enter the path to the Bank Folder: ");
            bankFolder = Console.ReadLine();

            Console.Write("Enter the path to output the FSPRO Project: ");
            outputProjectPath = Console.ReadLine();
        }

        // If user input nothing
        if (bankFolder == "")
        {
            Console.Write($"No Bank file path provided\nQuitting...");
            return;
        }
        if (outputProjectPath == "")
        {
            Console.Write($"No Output file path provided\nQuitting...");
            return;
        }

        // remove any qoutes in the strings, just in case
        bankFolder = bankFolder.Replace("\"", "");
        outputProjectPath = outputProjectPath.Replace("\"", "");

        // If bank folder doesn't exist
        if (!Directory.Exists(bankFolder))
        {
            PushToConsoleLog($"Bank Folder does not exist\nQuitting...", RED);
            return;
        }

        // If output folder doesn't exist, warn user
        if (!Directory.Exists(bankFolder))
        {
            PushToConsoleLog($"Output Folder does not exist", RED);
            PushToConsoleLog($"Continuing Anyways...", YELLOW);
        }

        #endregion

        // Get Project Name
        string projectname = "Generic-Project";
        Console.Write("Enter the Project Name: ");
        projectname = Console.ReadLine();
        var USESPACE = !verbose ? SPACE : "";

        if (projectname == "")
            projectname = "Generic-Project";

        #region Setup Output Folders

        // to ensure clean
        // yes it's pretty dumb, but i want it clean
        if (Directory.Exists(outputProjectPath))
            Directory.Delete(outputProjectPath, true);
        Directory.CreateDirectory(outputProjectPath);

        // Main Sub-Directories
        Directory.CreateDirectory(outputProjectPath + "/Assets");
        Directory.CreateDirectory(outputProjectPath + "/Metadata");

        // Sub-Directories of /Metadata
        Directory.CreateDirectory(outputProjectPath + "/Metadata/AudioFile");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/Asset");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/Bank");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/BankFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/EventFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/Platform");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/EncodingSetting");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/EffectPresetFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/ParameterPreset");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/ParameterPresetFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/ProfilerFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/SandboxFolder");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/SnapshotGroup");
        Directory.CreateDirectory(outputProjectPath + "/Metadata/Event");

        // Main FSPro File
        MasterXMLs.Create_FSPROFile(projectname);

        #endregion

        // create the FMOD Studio system
        FMOD.Studio.System studioSystem;
        FMOD.Studio.System.create(out studioSystem);
        studioSystem.initialize(512, INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

        PushToConsoleLog($"Loading Banks...", YELLOW);

        #region Built-in XML Files
        // this is basically just stuff that is ALWAYS gonna be in a FSPro Project

        // XML Files that are in their own subfolders
        MasterXMLs.Create_MasterAssetXML();
        MasterXMLs.Create_MasterBankFoldersXML();
        MasterXMLs.Create_MasterBankXML();
        MasterXMLs.Create_EventFolderXML();
        MasterXMLs.Create_PlatformXML();
        MasterXMLs.Create_EncodingSettingXML();
        MasterXMLs.Create_EffectPresetFolderXML();
        MasterXMLs.Create_ParameterPresetFolderXML();
        MasterXMLs.Create_ProfilerFolderXML();
        MasterXMLs.Create_SandboxFolderXML();
        MasterXMLs.Create_SnapshotGroupXML();

        // XML Files in the /Metadata folder
        MasterXMLs.Create_MasterXML();
        MasterXMLs.Create_MixerXML();
        MasterXMLs.Create_TagsXML();
        MasterXMLs.Create_WorkspaceXML();

        #endregion

        // load all the banks in the specified folder
        foreach (string bankFilePath in Directory.GetFiles(bankFolder, "*.bank"))
        {
            studioSystem.loadBankFile(bankFilePath, LOAD_BANK_FLAGS.NORMAL, out Bank bank);

            // just filename
            string bankfilename = Path.GetFileName(bankFilePath);
            PushToConsoleLog($"{USESPACE}\nLoaded Bank: {bankfilename}", GREEN);

            // if bank loaded is Master.strings.bank, stop and continue to next bank
            // as it never has anything useful to extract
            if (bankfilename == "Master.strings.bank")
                continue;

            // Extract Sounds to /Assets folder
            ExtractSoundAssets.ExtractSoundFiles(bankFilePath, bankfilename);

            // get the list of events in the bank
            bank.getEventCount(out int eventCount);
            PushToConsoleLog($"\nEvents Found in {bankFilePath}: {eventCount}\n", YELLOW, true);

            // basically just the XML Files for most assets that references their given bank file
            #region Bank Specific XMLs
            if (bankfilename != "Master.bank")// Master.bank has already been added, so skip it
            {
                // For Bank Asset XML
                BankSpecificGUIDs.Add(bankfilename + "_Asset", GetRandomGUID());
                MasterXMLs.Create_BankAssetXML(bankfilename);

                // For Bank File XML
                BankSpecificGUIDs.Add(bankfilename + "_Bank", GetRandomGUID());
                MasterXMLs.Create_BankFileXML(bankfilename);
            }
            #endregion

            // Start doing the actual extraction parts
            bank.getEventList(out EventDescription[] eventDescriptions);

            // clear organization everytime a bank file is loaded
            // so event:/music/folder and event:/sfx/folder dont merge to /music
            EventFolder.AllEvents.Clear();
            EventFolderGUIDs.Clear();

            #region Get Event Folders
            // if there are no events, just skip the entire bank
            if (eventDescriptions is null)
                continue;

            foreach (var eventDescription in eventDescriptions)
            {
                // get event path
                if (eventDescription.getPath(out string eventname) != FMOD.RESULT.OK)
                    continue;

                // Spinner for when --verbose was not used
                if (!verbose && SpinnerInit == false)
                {
                    // no await here, because we want it to continue
                    StartSpinnerAsync("Saving Events...", SpinnerPattern, 1000, SpinnerKill.Token);

                    // ensure this doesn't get called twice
                    SpinnerInit = true;
                }

                // add event name to save later
                EventFolder.AllEvents.Add(eventname);
            }
            // Extract Event Folders
            if (EventFolder.AllEvents.Count != 0)
                EventFolder.ExtractEventFolders(outputProjectPath + "/Metadata/EventFolder");
            #endregion

            // process each event in the bank
            foreach (var eventDescription in eventDescriptions)
            {
                // create event instance
                if (eventDescription.createInstance(out EventInstance eventInstance) != FMOD.RESULT.OK)
                    continue;

                // get event path
                if (eventDescription.getPath(out string eventname) != FMOD.RESULT.OK)
                    continue;

                // get event GUID
                if (eventDescription.getID(out FMOD.GUID eventID) != FMOD.RESULT.OK) 
                    continue;

                // make guid into a guid we can actually use, not fmod's bullshit
                Guid clean_eventID = FMODGUIDToSysGuid(eventID);

                PushToConsoleLog($"\nSaving Event: {eventname}", YELLOW, true);

                // add GUID to event
                EventGUIDs.TryAdd(eventname, clean_eventID); // you can get the GUID for a given event with EventGUIDs["event:/music/w2/graveyard"]

                // Add all events to txt
                File.AppendAllTextAsync(outputProjectPath + "/EventGUIDs.txt", $"\n{{{EventGUIDs[eventname]}}} {eventname}");

                #region Get Parameters
                if (FindEventType.EventisParameter(eventDescription))
                    FindEventType.GetParameterInfo(eventDescription);

                PushToConsoleLog($"Event GUID for {eventname}: {EventGUIDs[eventname]}");

                // event types
                if (FindEventType.EventisParameter(eventDescription))
                    PushToConsoleLog($"Event Sheet Type: Parameter\n{FindEventType.DisplayParameterInfo(eventDescription)}", OTHERGRAY, true);
                else if (FindEventType.EventisTimeline(eventInstance))
                    PushToConsoleLog($"Event Sheet Type: Timeline", OTHERGRAY, true);
                else
                    PushToConsoleLog($"Event Sheet Type: Action", OTHERGRAY, true);
                #endregion
                #region Get Internal Event MetaData
                // So basically what this is doing is that it's playing every sound in the event
                // so we can retrieve info on the sound such as sound file names
                // becase that's the only other way to extract this info for some reason

                #region Init These vars idk
                // force it to wait until event end callback returns
                bool Event_IsDone = false;

                // List that holds all names of sounds that have already been played
                List<string> SoundsinEvent = [];

                // List that holds all names of markers
                List<string> MarkersinEvent = [];

                // Save Sound Info to struct
                List<EventSoundInfo> SoundsInfo = [];
                EventSoundInfo SoundInfo;

                // Save Parameter Info to struct
                List<EventParameterInfo> ParametersInfo = [];
                EventParameterInfo ParameterInfo;

                // Save Marker Info to struct
                List<EventMarkerInfo> MarkersInfo = [];
                EventMarkerInfo MarkerInfo;

                // bool to check if it should be action
                // we determine this by seeing if a sound is less than a second
                // gotta do this because if its a timeline, it won't play in FMOD Studio
                bool IsAction = false;
                bool LockAction = false;

                // Sometimes Sound starts at 1.962 seconds late into the timeline
                // Adjust for that when needed
                bool AdjustStartPos = true;
                bool FirstSound = true;

                // Parameter stuffs
                bool IsParameter = FindEventType.EventisParameter(eventDescription);
                bool InitParameter = false;
                int ParameterValue = 0;
                int MaxParameterValue = 0;
                List<string> ParameterList = FindEventType.ParameterArray;
                int ParameterIndex = 0;
                string ParameterName = string.Empty;

                // if int is higher than 0, it loops
                Dictionary<string, int> SoundLoops = [];
                #endregion
                #region Callback
                // Here's basically all the Functions we can use now
                // https://www.fmod.com/docs/2.03/api/core-api-sound.html
                FMOD.RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr _unusedlmao, IntPtr parameterPtr)
                {
                    switch (type)
                    {
                        #region Sound Played Callback
                        // Callback that triggers once a single sound is played in the event
                        // (Can trigger many times depending on how many sounds there are)
                        case EVENT_CALLBACK_TYPE.SOUND_PLAYED:
                            #region Get Info
                            FMOD.Sound sound = new(parameterPtr);
                            if (sound.getName(out string name, 1024) != FMOD.RESULT.OK)
                            {
                                PushToConsoleLog($"ERROR! - Failed to get Sound Name!", RED, true);
                                break;
                            }

                            // Get Starting Position of sound currently playing
                            eventInstance.getTimelinePosition(out int currentPosition);

                            // get length of sound in milliseconds
                            sound.getLength(out uint soundlength, FMOD.TIMEUNIT.MS);

                            // get loop points of sound (aka start and end points on timeline)
                            sound.getLoopPoints(out uint unused, FMOD.TIMEUNIT.MS, out uint loopend, FMOD.TIMEUNIT.MS);

                            // Get File Extension (from ExtractSounds.cs)
                            var fileExtension = "";
                            // get sound names and their extensions from the current bank file
                            Dictionary<string, string> SoundNameExt = ExtractSoundAssets.SoundsinBanks[bankfilename];
                            // if sound was extracted and exists, get its extension
                            if (SoundNameExt.ContainsKey(name))
                                fileExtension = "." + SoundNameExt[name];

                            var truename = name + fileExtension;
                            // Get precise values (values with decimals)
                            double truelength = (double)soundlength / 1000;
                            double truestartpos = (double)currentPosition / 1000;
                            // Adjust Start Pos if needed
                            if (AdjustStartPos)
                            {
                                if (truestartpos != 0)
                                    truestartpos = truestartpos - 1.962;
                                // if First Sound starts at zero, dont adjust for this one or future ones
                                else if (truestartpos == 0 && FirstSound)
                                    AdjustStartPos = false;
                            }
                            FirstSound = false;
                            double truelooplength = (double)loopend / 1000;
                            #endregion
                            #region Set Info
                            // If Sound hasn't been played yet
                            if (!SoundsinEvent.Contains(name))
                            {
                                // Get Sound File used in Event
                                PushToConsoleLog($"\nSound Used: {truename}", GREEN, true);
                                PushToConsoleLog($"Sound Length: {truelength}", GREEN, true);
                                PushToConsoleLog($"Played at: {truestartpos}", GREEN, true);

                                if (IsParameter && ParameterValue > 0) 
                                {
                                    PushToConsoleLog($"Sound triggered on Parameter: {ParameterName}", GREEN, true);
                                    PushToConsoleLog($"Parameter Value when triggered: {ParameterValue}", GREEN, true);
                                }

                                // Add Important Sound Info to Struct
                                SoundInfo.name = truename;
                                SoundInfo.GUID = AudioFileGUIDs[bankfilename.Replace(".bank", "\\") + truename];
                                SoundInfo.startpos = truestartpos;
                                SoundInfo.length = truelength;
                                // Save info in a dictionary, since there could be many sounds
                                SoundsInfo.Add(SoundInfo);

                                // Add Parameter Info to Struct
                                if (ParameterName != string.Empty)
                                {
                                    ParameterInfo.name = ParameterName;
                                    ParameterInfo.GUID = Parameters.ParametersGuid[ParameterName];
                                    ParameterInfo.value = ParameterValue;
                                    ParameterInfo.start = truestartpos;
                                    ParameterInfo.length = truelooplength;
                                    ParametersInfo.Add(ParameterInfo);
                                }

                                // If sound is less than a second long, make it an Action Sheet
                                // Or else it won't play in FMOD Studio
                                if (!LockAction)
                                {
                                    if (SoundInfo.length < 1)
                                        IsAction = true;
                                    // If another sound in the same event is larger than that however, ensure it is Timeline
                                    else
                                    {
                                        IsAction = false;
                                        // ensure that IsAction can't be set to true anymore
                                        LockAction = true;
                                    }
                                }

                                // Flag as played
                                SoundsinEvent.Add(name);
                            }
                            #endregion
                            // to skip sound and go to the end of it for next sound
                            eventInstance.setTimelinePosition((int)loopend);

                            // do loop stuffs chud
                            if (SoundLoops.ContainsKey(truename))
                                SoundLoops[truename] = SoundLoops[truename]++;
                            else
                                SoundLoops.Add(truename, 0);

                            break;
                        #endregion
                        #region Marker Callback
                        // Callback for when it detects it passed a Marker
                        case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                            var marker = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
                            string markername = (string)marker.name;
                            double markerpos = (double)marker.position / 1000;
                            if (!MarkersinEvent.Contains(markername))
                            {
                                PushToConsoleLog($"Found Marker!", OTHERGRAY, true);
                                PushToConsoleLog($"Marker Name: {markername}", OTHERGRAY, true);
                                PushToConsoleLog($"Marker Pos: {markerpos}", OTHERGRAY, true);

                                if (IsParameter && ParameterValue > 0)
                                {
                                    PushToConsoleLog($"Marker triggered on Parameter: {ParameterName}", OTHERGRAY, true);
                                    PushToConsoleLog($"Parameter Value when triggered: {ParameterValue}", OTHERGRAY, true);
                                }

                                // Save Marker
                                MarkerInfo.name = markername;
                                MarkerInfo.position = markerpos;
                                MarkersInfo.Add(MarkerInfo);

                                MarkersinEvent.Add(markername);
                            }
                            break;
                        #endregion
                        #region Finish Event Callback
                        // Callback that triggers if the event has ended entirely (no more sounds have played)
                        case EVENT_CALLBACK_TYPE.STOPPED:
                            // Mark as done
                            // if its a parameter tho, wait for that check to finish
                            if (!IsParameter)
                                Event_IsDone = true;
                            break;
                        #endregion
                    }
                    return FMOD.RESULT.OK;
                }
                #endregion

                // Set Callback (Unified, because otherwise one of them wouldn't run)
                eventInstance.setCallback(EventCallback, EVENT_CALLBACK_TYPE.SOUND_PLAYED | EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.STOPPED);
                eventInstance.start();// Play Sound

                // Set volume to 0, because the following will kill your ears
                eventInstance.setVolume(0f);
                // Speed up Playback, because we don't care about actually listening to it
                var playbackSpeed = 100f;// should be 100x speed, thank god we aren't listening to it
                eventInstance.setPitch(playbackSpeed);

                // just in case it gets stuck
                int timeout = 0;
                // Get length of All of the Event's Audio
                eventDescription.getLength(out int EventLength);

                #region Initial Parameter Check n Set
                if (ParameterList == null || ParameterList.Count == 0)
                    IsParameter = false;

                if (IsParameter && !InitParameter)
                {
                    ParameterName = ParameterList[ParameterIndex];
                    PushToConsoleLog($"Checking Parameter: {ParameterName}", BROWN, true);
                    ParameterValue = FindEventType.GetMinParamValue(eventDescription, ParameterIndex);
                    MaxParameterValue = FindEventType.GetMaxParamValue(eventDescription, ParameterIndex);
                    // so this won't run anymore
                    InitParameter = true;
                }
                #endregion

                // Updates FMOD System until event has ended
                #region Wait for Callbacks
                while (!Event_IsDone)
                {
                    studioSystem.update();

                    // Timeout, that triggers when the event should've ended (accounting for speedup as well)
                    // plus about a second
                    if (timeout >= (EventLength / playbackSpeed) + 10000000)
                    {
                        #region Looping Parameter Check n Set
                        if (ParameterList == null || ParameterList.Count == 0)
                            IsParameter = false;

                        if (IsParameter && !InitParameter)
                        {
                            ParameterName = ParameterList[ParameterIndex];
                            PushToConsoleLog($"Checking Parameter: {ParameterName}", BROWN, true);
                            ParameterValue = FindEventType.GetMinParamValue(eventDescription, ParameterIndex);
                            MaxParameterValue = FindEventType.GetMaxParamValue(eventDescription, ParameterIndex);
                            // so this won't run anymore
                            InitParameter = true;
                        }
                        #endregion

                        // If not Parameter
                        if (!IsParameter)
                        {
                            // if event just had no audio files in it
                            if (EventLength == 0 && SoundsinEvent.Count == 0)
                                PushToConsoleLog($"ERROR! - Event has no audio!", RED, true);
                            // if no sounds played at all, but the event still has length
                            else if (SoundsinEvent.Count == 0 && EventLength != 0)
                                PushToConsoleLog($"ERROR! - Internal Event Metadata failed to load!", RED, true);
                            else
                                PushToConsoleLog($"Internal Event Checking Timed Out...", NORMAL, true);

                            break;
                        }
                        // If Parameter Value hasn't reached its Max, go onto next value
                        else if (MaxParameterValue > ParameterValue && IsParameter)
                        {
                            ParameterValue++;
                            PushToConsoleLog($"Setting value for Parameter \"{ParameterName}\" to: {ParameterValue}", BROWN);
                            eventInstance.setParameterByName(ParameterName, ParameterValue);
                            // reset timer
                            timeout = 0;
                        }
                        // If Parameter Value has reached its end
                        else if (MaxParameterValue == ParameterValue && IsParameter)
                        {
                            // if no more parameters to check, leave cycle
                            if ((ParameterList.Count - 1) <= ParameterIndex)
                                break;
                            else
                            {
                                // move onto next parameter
                                ParameterIndex++;
                                // redo the cycle
                                InitParameter = false;
                                // reset timer
                                timeout = 0;
                            }
                        }
                    }
                    timeout++;
                }
                #endregion

                // Stop sound if the while loop condition is met
                // aka if it finishes extracting shit
                eventInstance.stop(STOP_MODE.IMMEDIATE);
                eventInstance.release();
                #endregion

                // EventisTimeline() func checks it in a way that Action Sheets return false to
                // so might as well use it, plus add the other check
                IsAction = FindEventType.EventisTimeline(eventInstance) ? IsAction : true;

                // Save Event XML
                Events.SaveEvents(eventname, bankfilename, SoundsInfo, MarkersInfo, ParametersInfo, SoundLoops, IsAction);
            }
        }

        #region Finish
        // if not verbose, stop spinner
        if (!verbose)
            SpinnerKill.Cancel();

        PushToConsoleLog($"{USESPACE}\nConversion Complete!", GREEN);
        PushToConsoleLog($"Exported Project is at {outputProjectPath}", GREEN);

        // Clean up the FMOD Studio system
        studioSystem.release();
        #endregion
    }

    // If User is not using --verbose
    #region Spinner
    public static async Task StartSpinnerAsync(string displayMsg = "", int sequenceCode = 0, int delay = 1000, CancellationToken cancellationToken = default)
    {
        int counter = 0;
        string[,] sequence = new string[,] {
            { "/", "-", "\\", "|" },
            { ".   ", "..  ", "... ", "...." },
            { "|=   |", "|==  |", "|=== |", "|====|" },
        };

        int totalSequences = sequence.GetLength(0);

        try
        {
            while (true)
            {
                // check if spinner has been cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // progress frame
                counter++;

                // Delay
                await Task.Delay(delay, cancellationToken);

                sequenceCode = sequenceCode > totalSequences - 1 ? 0 : sequenceCode;
                int counterValue = counter % 4;

                // make full spinner message
                string fullMessage = displayMsg + "    " + sequence[sequenceCode, counterValue];

                // ensure last line is clear
                Console.Write("\r                                                    ");

                // Write the new spinner message while clearing last line
                Console.Write("\r" + fullMessage);

                // Ensure the cursor is positioned at the start for the next loop
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }
        catch (OperationCanceledException) { }
    }
    #endregion
}