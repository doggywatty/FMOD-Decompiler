using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using FMOD;
using FMOD.Studio;
// shit that extracts sounds from banks
// https://github.com/SamboyCoding/Fmod5Sharp
using Fmod5Sharp;
using System.Text;
using Fmod5Sharp.FmodTypes;

namespace BankToFSPro
{
    class Program
    {
        #region Colored Text
        // thank you https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        public static string NL = Environment.NewLine; // shortcut
        public static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
        public static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
        public static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
        public static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
        public static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
        public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
        public static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
        public static string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
        public static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
        public static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
        public static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
        public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
        public static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
        public static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";

        // and thank you https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);
        #endregion

        // get random GUIDs for some stuff
        static Guid GetRandomGUID()
        {
            // Generate a new GUID
            Guid newGuid = Guid.NewGuid();
            return newGuid;
        }
        // since they are static, it'll only run once, so they should stay the same
        public static Guid MasterAssetsGUID = GetRandomGUID();
        public static Guid MasterBankFolderGUID = GetRandomGUID();
        // only temporary
        public static string TEMP_GUID = "00000000-0000-0000-0000-000000000000";

        static void Main(string[] args)
        {
            // initialize
            GetConsoleMode(GetStdHandle(-11), out int mode);
            SetConsoleMode(GetStdHandle(-11), mode | 0x4);
            Console.Clear();

            Console.WriteLine($"Welcome to the FMOD Bank Decompiler {RED}(WIP Version){NORMAL}"
            + $"\n\nby {CYAN}CatMateo{NORMAL}"
            + $"\nand {RED}burnedpopcorn180{NORMAL}"

            + $"\n\n{RED}THIS IS STILL VERY WIP{NORMAL}"
            + $"\n{RED}AND IS CURRENTLY NON-FUNCTIONAL AS A DECOMPILER{NORMAL}"

            + $"\n"
            );

            string bankFolder = "";
            string outputProjectPath = "";
            bool verbose = false;

            // Long ass shit that we should just ignore
            #region DLLs

            // kinda shit way of doing it, but i really dont want to fuck with the FMOD code just to dynamically link it
            if (!File.Exists(@"C:\fmod-decompiler\fmod.dll"))
            {
                string backupfmodDLL = Path.GetDirectoryName(Environment.ProcessPath) + @"\dlls\fmod.dll"; // Path to your DLL
                try {
                    // Check if the fmod.dll exists
                    if (File.Exists(backupfmodDLL))
                    {
                        // Copy the file to System32
                        Directory.CreateDirectory(@"C:\fmod-decompiler\");
                        File.Copy(backupfmodDLL, @"C:\fmod-decompiler\fmod.dll");

                        Console.WriteLine($"{GREEN}fmod.dll was applied!{NORMAL}");
                    }
                    else
                    {
                        Console.WriteLine($"{RED}fmod.dll was not found in the /dlls folder.{NORMAL}");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"{RED}fmod.dll could not be copied to System\nPlease restart the program as an administrator{NORMAL}");
                    return;
                }
            }

            if (!File.Exists(@"C:\fmod-decompiler\fmodstudio.dll"))
            {
                string backupfmodstudioDLL = Path.GetDirectoryName(Environment.ProcessPath) + @"\dlls\fmodstudio.dll"; // Path to your DLL
                try
                {
                    // Check if the fmod.dll exists
                    if (File.Exists(backupfmodstudioDLL))
                    {
                        // Copy the file to System32
                        Directory.CreateDirectory(@"C:\fmod-decompiler\");
                        File.Copy(backupfmodstudioDLL, @"C:\fmod-decompiler\fmodstudio.dll");

                        Console.WriteLine($"{GREEN}fmodstudio.dll was applied!{NORMAL}");
                    }
                    else
                    {
                        Console.WriteLine($"{RED}fmodstudio.dll was not found in the /dlls folder.{NORMAL}");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"{RED}fmodstudio.dll could not be copied to System\nPlease restart the program as an administrator{NORMAL}");
                    return;
                }
            }

            #endregion
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
                    Console.WriteLine($"{RED}Missing argument: {args[i]}{NORMAL}");
                    return;
                }
            }

            // if no arguments were added
            if (args.Length == 0)
            {
                Console.Write("Enter the path to the bank folder: ");
                bankFolder = Console.ReadLine();

                Console.Write("Enter the path to output the FSPro project: ");
                outputProjectPath = Console.ReadLine();
            }

            // If user input nothing
            if (bankFolder == "") 
            {
                Console.WriteLine($"{RED}No Bank file path provided\nQuitting...{NORMAL}");
                return;
            }
            if (outputProjectPath == "")
            {
                Console.WriteLine($"{RED}No Output file path provided\nQuitting...{NORMAL}");
                return;
            }

            // remove any qoutes in the strings, just in case
            bankFolder = bankFolder.Replace("\"", "");
            outputProjectPath = outputProjectPath.Replace("\"", "");

            // If bank folder doesn't exist
            if (!Directory.Exists(bankFolder)) 
            {
                Console.WriteLine($"{RED}Bank Folder does not exist\nQuitting...{NORMAL}");
                return;
            }

            // If output folder doesn't exist, warn user
            if (!Directory.Exists(bankFolder))
                Console.WriteLine($"{RED}Output Folder does not exist{NORMAL}\n{YELLOW}Continuing Anyways...{NORMAL}");

            #endregion

            // Get Project Name
            string projectname = "Generic-Project";
            Console.Write("Enter the Project Name: ");
            projectname = Console.ReadLine();

            if (projectname == "") 
                projectname = "Generic-Project";

            #region Setup Output Folders

            // to ensure clean
            // yes it's pretty dumb, but i want it clean
            if (Directory.Exists(outputProjectPath))
                Directory.Delete(outputProjectPath, true);
            Directory.CreateDirectory(outputProjectPath);

            Directory.CreateDirectory(outputProjectPath + "/Assets");
            Directory.CreateDirectory(outputProjectPath + "/Metadata");

            // Sub-Directories of /Metadata (ADD THE REST WHEN YOU FINISH THE XML ONES THAT NEED TO GO HERE)
            Directory.CreateDirectory(outputProjectPath + "/Metadata/AudioFile");
            Directory.CreateDirectory(outputProjectPath + "/Metadata/Asset");
            Directory.CreateDirectory(outputProjectPath + "/Metadata/Bank");
            Directory.CreateDirectory(outputProjectPath + "/Metadata/BankFolder");

            // because i can
            File.AppendAllText(outputProjectPath + $"/{projectname}.fspro", ""
                + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<objects serializationModel=\"Studio.02.02.00\" />");

            #endregion

            // create the FMOD Studio system
            FMOD.Studio.System studioSystem;
            FMOD.Studio.System.create(out studioSystem);
            studioSystem.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

            if (!verbose)
                Console.WriteLine($"{YELLOW}Finding and Saving Events...{NORMAL}");

            #region Built-in XML Files
            // this is basically just stuff that is ALWAYS gonna be in a FSPro Project
            // and needs to be set, because GUID reasons

            // also this is more readable than trying any XML type shit LMAO

            // For Master Asset XML
            File.WriteAllText(outputProjectPath + $"/Metadata/Asset/{{{MasterAssetsGUID}}}.xml", ""
                + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r"
                + "\n<objects serializationModel=\"Studio.02.02.00\">\r"
                + $"\n\t<object class=\"MasterAssetFolder\" id=\"{{{MasterAssetsGUID}}}\" />\r"
                + "\n</objects>");

            // For Master Bank XML (Bank Folders)
            File.WriteAllText(outputProjectPath + $"/Metadata/BankFolder/{{{MasterBankFolderGUID}}}.xml", ""
                + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r"
                + "\n<objects serializationModel=\"Studio.02.02.00\">\r"
                + $"\n\t<object class=\"MasterBankFolder\" id=\"{{{MasterBankFolderGUID}}}\" />\r\n</objects>");

            // For Master Bank XML
            File.WriteAllText(outputProjectPath + $"/Metadata/Bank/{{{TEMP_GUID}}}.xml", ""
                + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r"
                + "\n<objects serializationModel=\"Studio.02.02.00\">\r"
                + $"\n\t<object class=\"Bank\" id=\"{{{TEMP_GUID}}}\">\r"
                + "\n\t\t<property name=\"name\">\r\n\t\t\t<value>Master</value>\r\n\t\t</property>\r"
                + "\n\t\t<property name=\"isMasterBank\">\r\n\t\t\t<value>true</value>\r\n\t\t</property>\r"
                + $"\n\t\t<relationship name=\"folder\">\r\n\t\t\t<destination>{{{MasterBankFolderGUID}}}</destination>\r"
                + "\n\t\t</relationship>\r\n\t</object>\r\n</objects>");

            #endregion

            // load all the banks in the specified folder
            foreach (string bankFilePath in Directory.GetFiles(bankFolder, "*.bank"))
            {
                FMOD.Studio.Bank bank;
                studioSystem.loadBankFile(bankFilePath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

                // just filename
                string bankfilename = Path.GetFileName(bankFilePath);

                // get the list of events in the bank
                int eventCount;
                bank.getEventCount(out eventCount);
                if (verbose)
                    Console.WriteLine($"\n{YELLOW}Events Found in {bankFilePath}: {eventCount}{NORMAL}\n");

                // if bank with events/music (music.bank and sfx.bank), then add reference to its assets
                if (eventCount > 0) 
                {
                    // For Bank Asset XML
                    File.WriteAllText(outputProjectPath + $"/Metadata/Asset/{{{TEMP_GUID}}}_{bankfilename}.xml", ""
                        + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r"
                        + "\n<objects serializationModel=\"Studio.02.02.00\">\r"
                        + $"\n\t<object class=\"EncodableAsset\" id=\"{{{TEMP_GUID}}}\">\r"
                        + "\n\t\t<property name=\"assetPath\">\r"
                        + $"\n\t\t\t<value>{bankfilename.Replace(".bank","")}/</value>\r\n\t\t</property>\r"
                        + "\n\t\t<relationship name=\"masterAssetFolder\">\r"
                        + $"\n\t\t\t<destination>{{{MasterAssetsGUID}}}</destination>\r"
                        + "\n\t\t</relationship>\r\n\t</object>\r\n</objects>");

                    // For Bank XML
                    File.WriteAllText(outputProjectPath + $"/Metadata/Bank/{{{TEMP_GUID}}}_{bankfilename}.xml", ""
                        + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<objects serializationModel=\"Studio.02.02.00\">\r"
                        + $"\n\t<object class=\"Bank\" id=\"{{{TEMP_GUID}}}\">\r\n\t\t<property name=\"name\">\r"
                        + $"\n\t\t\t<value>{bankfilename.Replace(".bank", "")}</value>\r\n\t\t</property>\r\n\t\t<relationship name=\"folder\">\r"
                        + $"\n\t\t\t<destination>{{{MasterBankFolderGUID}}}</destination>\r\n\t\t</relationship>\r\n\t</object>\r\n</objects>");
                }

                FMOD.Studio.EventDescription[] eventDescriptions = new FMOD.Studio.EventDescription[eventCount];
                bank.getEventList(out eventDescriptions);

                // process each event in the bank (this is a placeholder, actual processing logic may vary)
                foreach (var eventDescription in eventDescriptions)
                {
                    FMOD.Studio.EventInstance eventInstance;
                    eventDescription.createInstance(out eventInstance);

                    // get event name
                    eventDescription.getPath(out string eventname);
                    // add event name to save later
                    EventFolder.AllEvents.Add(eventname);

                    // save the event instance to the project (this is a placeholder, actual saving logic may vary)
                    if (verbose)
                        Console.WriteLine($"{YELLOW}Saving Event: {eventname}{NORMAL}");
                    SaveEventInstance(eventInstance, eventDescription, outputProjectPath);
                }

                // Extract Sounds to /Assets folder
                ExtractSoundFiles(bankFilePath, outputProjectPath + "/Assets", bankfilename, verbose);
                // Extract Event Folders
                EventFolder.ExtractEventFolders();
            }

            Console.WriteLine($"\n{GREEN}Conversion Complete!{NORMAL}");
            Console.WriteLine($"{GREEN}Exported Project is at {outputProjectPath}{NORMAL}");

            // Clean up the FMOD Studio system
            studioSystem.release();
        }

        static void SaveEventInstance(FMOD.Studio.EventInstance eventInstance, FMOD.Studio.EventDescription eventDescription, string outputProjectPath)
        {
            // implement the logic to save the event instance to the specified path
            // this is a placeholder implementation
            // example: Serialize event instance data to a file in the output project path
        }

        // from https://github.com/SamboyCoding/Fmod5Sharp/blob/master/BankExtractor/Program.cs
        static void ExtractSoundFiles(string bankPath, string outPath, string bankfilename, bool verbose)
        {
            // if Master.bank or Master.strings.bank
            if (bankPath.Contains("Master"))
                return; // ignore because it causes the thing to fail

            var bytes = File.ReadAllBytes(bankPath);
            var index = bytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("FSB5"));
            if (index > 0)
            {
                bytes = bytes.AsSpan(index).ToArray();
            }
            var bank = FsbLoader.LoadFsbFromByteArray(bytes);
            var outDir = Directory.CreateDirectory(outPath + $"/{ bankfilename.Replace(".bank", "")}/");

            if (verbose)
                Console.WriteLine($"\n{YELLOW}Extracting Sound Files from {bankfilename}...{NORMAL}\n");

            var i = 0;
            foreach (var bankSample in bank.Samples)
            {
                i++;
                var name = bankSample.Name ?? $"UnknownSound-{i}";

                if (!bankSample.RebuildAsStandardFileFormat(out var data, out var extension))
                {
                    Console.WriteLine($"{RED}Failed to Extract Sound {name}{NORMAL}");
                    continue;
                }

                var filePath = Path.Combine(outDir.FullName, $"{name}.{extension}");
                File.WriteAllBytes(filePath, data);
                if (verbose)
                    Console.WriteLine($"{CYAN}Extracted Sound {name}.{extension}{NORMAL}");

                // add to xml
                List<FmodSample> samples = bank.Samples;
                // because it sometimes fails idk
                // also DEAR GOD
                try
                {
                    int frequency = samples[i].Metadata.Frequency; //E.g. 44100
                    uint numChannels = samples[i].Metadata.Channels; //2 for stereo, 1 for mono.
                    AudioFile.AudioFileXML(outPath, filePath, bankfilename, frequency, numChannels);
                }
                catch (Exception)
                {
                    try 
                    {
                        int frequency = samples[i].Metadata.Frequency; //E.g. 44100
                        AudioFile.AudioFileXML(outPath, filePath, bankfilename, frequency, 2);
                    } 
                    catch (Exception) 
                    {
                        try 
                        {
                            uint numChannels = samples[i].Metadata.Channels; //2 for stereo, 1 for mono.
                            AudioFile.AudioFileXML(outPath, filePath, bankfilename, 44100, numChannels);
                        } 
                        catch (Exception) 
                        {
                            AudioFile.AudioFileXML(outPath, filePath, bankfilename, 44100, 2);
                        }
                    }
                }
            }
        }
    }
}

/*

SO
notes by the burned shit
and its probably shit

the /Assets folder
just contains all sound files
kinda obvious, and seems to be in folders that have the same name as the event root

so like (event:/music -> /music)
so we could possibly just use the name of the .bank as the folder name
or just get that part of the event using regex i guess

the /Metadata folder
the jouicy stuffs

there ARE some folders that are empty sometimes, but FMOD just adds them if missing anyways

the folders that hold the base info (as in this stuff you would see in a template) are:
/Asset                          (Has all of the subdirectories in /Assets (with one for Main))
/AudioFile                      (Has ALL Audio Files in /Assets listed (with settings like length listed))
/Bank                           (Has all bank file names (ex: music and sfx))
/BankFolder                     (Just links to master bank file i think)
/EncodingSetting                (Audio Format and Quality settings for the entire project)
/Event                          (Has EACH INDIVIDUAL EVENT and its settings)
/EventFolder                    (Has the Event Folders (in this example its "w1": event:/music/w1/ruintitle)
/Group                          (idk, this shit too dense for me)
/Platform                       (just the output folder lmao)

the rest seem to be bank specific

also
some lone .xml files are here as well
these are pretty small tho

Master.xml                      (its like /Group, kinda i guess)
Mixer.xml                       (has masterbus and snapshotList (whatever that means))
Tags.xml                        ()
Workspace.xml

*/