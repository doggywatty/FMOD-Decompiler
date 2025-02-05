using System;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using FMOD;
using FMOD.Studio;

namespace BankToFSPro
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize
            Console.WriteLine("Welcome to the FMOD Bank Decompiler (WIP Version)"
            + "\n\nby CatMateo"
            + "\nand burnedpopcorn180"

            + "\n\nTHIS IS STILL VERY WIP"
            + "\nAND IS CURRENTLY NON-FUNCTIONAL"

            + "\n"
            );

            string bankFolder = "";
            string outputProjectPath = "";
            bool verbose = false;

            // Long ass shit that we should just ignore
            #region DLLs

            // kinda shit way of doing it, but i really dont want to fuck with the FMOD code just to dynamically link it
            if (!File.Exists(@"C:\Windows\System32\fmod.dll"))
            {
                string backupfmodDLL = Path.GetDirectoryName(Environment.ProcessPath) + @"dlls\fmod.dll"; // Path to your DLL
                try {
                    // Check if the fmod.dll exists
                    if (File.Exists(backupfmodDLL))
                    {
                        // Copy the file to System32
                        File.Copy(backupfmodDLL, @"C:\Windows\System32\fmod.dll");

                        // check if this isn't is missing, so we can just end execution
                        // if its missing tho, we move on to the next dll
                        if (File.Exists(@"C:\Windows\System32\fmodstudio.dll"))
                        {
                            Console.WriteLine("fmod.dll was applied!\nPlease restart the program for changes to take effect");
                            return;
                        }
                        else 
                        {
                            Console.WriteLine("fmod.dll was applied!");
                            // dont stop execution, since if we're here, the other one needs to be added
                        }
                    }
                    else
                    {
                        Console.WriteLine("fmod.dll was not found in the /dlls folder.");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("fmod.dll could not be copied to System\nPlease restart the program as an administrator");
                    return;
                }
            }

            if (!File.Exists(@"C:\Windows\System32\fmodstudio.dll"))
            {
                string backupfmodstudioDLL = Path.GetDirectoryName(Environment.ProcessPath) + @"dlls\fmodstudio.dll"; // Path to your DLL
                try
                {
                    // Check if the fmod.dll exists
                    if (File.Exists(backupfmodstudioDLL))
                    {
                        // Copy the file to System32
                        File.Copy(backupfmodstudioDLL, @"C:\Windows\System32\fmodstudio.dll");

                        // we can just end execution, since it was already checked above
                        Console.WriteLine("fmodstudio.dll was applied!\nPlease restart the program for changes to take effect");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("fmodstudio.dll was not found in the /dlls folder.");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("fmodstudio.dll could not be copied to System\nPlease restart the program as an administrator");
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
                    Console.WriteLine($"Missing argument: {args[i]}");
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
                Console.WriteLine("No Bank file path provided\nQuitting...");
                return;
            }
            if (outputProjectPath == "")
            {
                Console.WriteLine("No Output file path provided\nQuitting...");
                return;
            }

            // remove any qoutes in the strings, just in case
            bankFolder = bankFolder.Replace("\"", "");
            outputProjectPath = outputProjectPath.Replace("\"", "");

            // If bank folder doesn't exist
            if (!Directory.Exists(bankFolder)) 
            {
                Console.WriteLine("Bank Folder does not exist\nQuitting...");
                return;
            }

            // If output folder doesn't exist, warn user
            if (!Directory.Exists(bankFolder))
                Console.WriteLine("Output Folder does not exist\nContinuing Anyways...");

            #endregion

            // Get Project Name
            string projectname = "Generic-Project";
            Console.Write("Enter the Project Name: ");
            projectname = Console.ReadLine();

            #region Setup Output Folders

            // If output folder doesn't exist, make it
            if (!Directory.Exists(outputProjectPath))
                Directory.CreateDirectory(outputProjectPath);

            Directory.CreateDirectory(outputProjectPath + "/Assets");
            Directory.CreateDirectory(outputProjectPath + "/Metadata");
            // because i can
            File.AppendAllText(outputProjectPath + $"/{projectname}.fspro", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<objects serializationModel=\"Studio.02.02.00\" />");

            #endregion

            // create the FMOD Studio system
            FMOD.Studio.System studioSystem;
            FMOD.Studio.System.create(out studioSystem);
            studioSystem.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

            if (!verbose)
                Console.WriteLine("Finding and Saving Events....");

            // load all the banks in the specified folder
            foreach (string bankFilePath in Directory.GetFiles(bankFolder, "*.bank"))
            {
                FMOD.Studio.Bank bank;
                studioSystem.loadBankFile(bankFilePath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

                // get the list of events in the bank
                int eventCount;
                bank.getEventCount(out eventCount);
                if (verbose)
                    Console.WriteLine($"\nEvents Found in {bankFilePath}: {eventCount}\n");

                FMOD.Studio.EventDescription[] eventDescriptions = new FMOD.Studio.EventDescription[eventCount];
                bank.getEventList(out eventDescriptions);

                // process each event in the bank (this is a placeholder, actual processing logic may vary)
                foreach (var eventDescription in eventDescriptions)
                {
                    FMOD.Studio.EventInstance eventInstance;
                    eventDescription.createInstance(out eventInstance);

                    // get event name
                    eventDescription.getPath(out string eventname);

                    // save the event instance to the project (this is a placeholder, actual saving logic may vary)
                    if (verbose)
                        Console.WriteLine($"Saving Event: {eventname}");
                    SaveEventInstance(eventInstance, eventDescription, outputProjectPath);
                }
            }

            Console.WriteLine("\nConversion complete!");

            // Clean up the FMOD Studio system
            studioSystem.release();
        }

        static void SaveEventInstance(FMOD.Studio.EventInstance eventInstance, FMOD.Studio.EventDescription eventDescription, string outputProjectPath)
        {
            // implement the logic to save the event instance to the specified path
            // this is a placeholder implementation
            // example: Serialize event instance data to a file in the output project path
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