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
    public static Dictionary<string, Guid> EventGUIDs = new Dictionary<string, Guid> { };
    public static Dictionary<string, Guid> EventFolderGUIDs = new Dictionary<string, Guid> { };
    public static Dictionary<string, Guid> AudioFileGUIDs = new Dictionary<string, Guid> { };
    public static Dictionary<string, Guid> BankSpecificGUIDs = new Dictionary<string, Guid> { };
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

    public static async Task Main(string[] args)
    {
        // initialize
        GetConsoleMode(GetStdHandle(-11), out int mode);
        SetConsoleMode(GetStdHandle(-11), mode | 0x4);
        Console.Clear();

        Console.WriteLine($"Welcome to the FMOD Bank Decompiler {GREEN}(Version 1.2.0){NORMAL}"
        + $"\n\nby {BROWN}DogMatt{NORMAL}"
        + $"\nand {OTHERGRAY}burnedpopcorn180{NORMAL}"

        + $"\n\n{RED}Unfortunately, this Decompiler is pretty limited in what it can extract{NORMAL}"
        + $"\n{RED}Most things, like Events, you WILL have to recreate by yourself{NORMAL}"
        + $"\n{RED}But this Decompiler will at least give you a bare shell to work off of{NORMAL}"

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
            EventFolder.processedFolders.Clear();
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

                // HOLY SHIT THIS WORKS
                // THE FLOOD GATES HAVE OPENED
                #region Get Internal Event MetaData
                // So basically what this is doing is that it's playing every sound in the event
                // so we can retrieve info on the sound such as sound file names
                // becase that's the only other way to extract this info for some reason

                // force it to wait until event end callback returns
                bool Event_IsDone = false;

                // List that holds all names of sounds that have already been played
                List<string> SoundsinEvent = new List<string> { };

                #region Callbacks
                // Here's basically all the Functions we can use now
                // https://www.fmod.com/docs/2.01/api/core-api-sound.html
                FMOD.RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr _unusedlmao, IntPtr parameterPtr)
                {
                    switch (type)
                    {
                        // Callback that triggers once a single sound is played in the event
                        // (Can trigger many times depending on how many sounds there are)
                        case EVENT_CALLBACK_TYPE.SOUND_PLAYED:

                            FMOD.Sound sound = new(parameterPtr);
                            if (sound.getName(out string name, 1024) != FMOD.RESULT.OK)
                            {
                                PushToConsoleLog($"ERROR! - Failed to get Sound Name!", RED, true);
                                break;
                            }

                            // Get File Extension (from ExtractSounds.cs)
                            var fileExtension = "";
                            // get sound names and their extensions from the current bank file
                            Dictionary<string, string> SoundNameExt = ExtractSoundAssets.SoundsinBanks[bankfilename];
                            // if sound was extracted and exists, get its extension
                            if (SoundNameExt.ContainsKey(name))
                                fileExtension = "." + SoundNameExt[name];

                            // If Sound hasn't been played yet
                            if (!SoundsinEvent.Contains(name))
                            {
                                // Get Sound File used in Event
                                PushToConsoleLog($"Sound Used: {name}{fileExtension}", GREEN, true);
                                // Flag as played
                                SoundsinEvent.Add(name);
                            }
                            // Just leave if it has already been played
                            break;

                        // Callback that triggers if the event has ended entirely (no more sounds have played)
                        case EVENT_CALLBACK_TYPE.STOPPED:
                            // Mark as done
                            Event_IsDone = true;
                            break;
                    }
                    return FMOD.RESULT.OK;
                }
                #endregion

                // Set Callback (Unified, because otherwise one of them wouldn't run)
                eventInstance.setCallback(EventCallback, EVENT_CALLBACK_TYPE.SOUND_PLAYED | EVENT_CALLBACK_TYPE.STOPPED);
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

                // Updates FMOD System until event has ended
                while (!Event_IsDone)
                {
                    studioSystem.update();

                    // Timeout, that triggers when the event should've ended (accounting for speedup as well)
                    // plus about a second
                    if (timeout == (EventLength / playbackSpeed) + 10000000)
                    {
                        // if event just had no audio files in it
                        if (EventLength == 0 && SoundsinEvent.Count == 0)
                            PushToConsoleLog($"ERROR! - Event has no audio!", RED, true);
                        // if no sounds played at all, but the event still has length
                        else if (SoundsinEvent.Count == 0 && EventLength != 0)
                            PushToConsoleLog($"ERROR! - Internal Event Metadata failed to load!", RED, true);
                        else
                            PushToConsoleLog($"Internal Event Checking Timed Out...\n(Likely a Looping Event)", NORMAL, true);

                        break;
                    }
                    timeout++;
                }

                // Stop sound if the while loop condition is met
                // aka if it finishes extracting shit
                eventInstance.stop(STOP_MODE.IMMEDIATE);
                eventInstance.release();
                #endregion

                // Save Event XML
                Events.SaveEvents(eventname, bankfilename);
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