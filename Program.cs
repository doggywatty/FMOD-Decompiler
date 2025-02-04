using System;
using System.IO;
using FMOD;
using FMOD.Studio;

namespace BankToFSPro
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the path to the bank folder: ");
            string bankFolder = Console.ReadLine();

            Console.Write("Enter the path to output the FSPro project: ");
            string outputProjectPath = Console.ReadLine();

            // If user input nothing
            if (bankFolder == null || outputProjectPath == null) 
            {
                Console.WriteLine("Input File paths are empty\nQuitting...");
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

            // create the FMOD Studio system
            FMOD.Studio.System studioSystem;
            FMOD.Studio.System.create(out studioSystem);// THE ONE WITH THE ISSUES
            studioSystem.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

            // load all the banks in the specified folder
            foreach (string bankFilePath in Directory.GetFiles(bankFolder, "*.bank"))
            {
                FMOD.Studio.Bank bank;
                studioSystem.loadBankFile(bankFilePath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

                // get the list of events in the bank
                int eventCount;
                bank.getEventCount(out eventCount);

                FMOD.Studio.EventDescription[] eventDescriptions = new FMOD.Studio.EventDescription[eventCount];
                bank.getEventList(out eventDescriptions);

                // process each event in the bank (this is a placeholder, actual processing logic may vary)
                foreach (var eventDescription in eventDescriptions)
                {
                    FMOD.Studio.EventInstance eventInstance;
                    eventDescription.createInstance(out eventInstance);

                    // save the event instance to the project (this is a placeholder, actual saving logic may vary)
                    SaveEventInstance(eventInstance, outputProjectPath);
                }
            }

            Console.WriteLine("Conversion complete!");

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