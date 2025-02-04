using System;
using System.Diagnostics.Tracing;
using System.IO;
using FMOD;
using FMOD.Studio;

namespace BankToFSPro
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize
            string bankFolder = "";
            string outputProjectPath = "";
            bool verbose = false;

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

                    // save the event instance to the project (this is a placeholder, actual saving logic may vary)
                    if (verbose)
                        Console.WriteLine($"Saving Event: {eventDescription}");// FIX THIS
                    SaveEventInstance(eventInstance, outputProjectPath);
                }
            }

            Console.WriteLine("\nConversion complete!");

            // Clean up the FMOD Studio system
            studioSystem.release();
        }

        static void SaveEventInstance(FMOD.Studio.EventInstance eventInstance, string outputProjectPath)
        {
            // implement the logic to save the event instance to the specified path
            // this is a placeholder implementation
            // example: Serialize event instance data to a file in the output project path
        }
    }
}