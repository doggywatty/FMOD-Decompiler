using Fmod5Sharp.FmodTypes;
using FModBankParser;
using FModBankParser.Nodes;
using FModBankParser.Objects;
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
	public static CancellationTokenSource SpinnerKill = new();

	// Argument Values
	public static string bankFolder = "";
	public static string outputProjectPath = "";
	public static string projectname = "Generic-Project";
	public static bool verbose = false;
	public static bool IsGUI = false;

	public static FRadixTreePacked? StringTable = null;
    #endregion

    #region Helper Funcs
    public static void PushToConsoleLog(string message, string color = "NONE", bool toLog = false)
	{
		// for some reason I can't just string color = NORMAL at the beginning because compiler cries
		var truecolor = (color == "NONE" || IsGUI) ? NORMAL : color;

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
			Console.WriteLine($"{truecolor}{message}{(!IsGUI ? NORMAL : "")}");

		// If also saving to log
		if (toLog)
			File.AppendAllTextAsync(outputProjectPath + "/log.txt", "\n" + message);
 
	}

	// Create Random GUIDs
	public static Guid GetRandomGUID() {
		return Guid.NewGuid();
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
		#region Check Arguments
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--input" when i + 1 < args.Length:
					bankFolder = args[++i];
					break;
				case "--output" when i + 1 < args.Length:
					outputProjectPath = args[++i];
					break;
				case "--name" when i + 1 < args.Length:
					projectname = args[++i];
					break;
				case "--verbose":
					verbose = true;
					break;
				case "--GUI":
					IsGUI = true;
					break;
				default:
					Console.WriteLine($"Invalid or missing value for argument: {args[i]}");
					return;
			}
		}
		#endregion

		// initialize
		if (!IsGUI)
		{
			GetConsoleMode(GetStdHandle(-11), out int mode);
			SetConsoleMode(GetStdHandle(-11), mode | 0x4);
			Console.Clear();
		}

		Console.WriteLine($"Welcome to the FMOD Bank Decompiler {GREEN}(Version 1.4.4){NORMAL}"
		+ $"\n\nby {OTHERGRAY}burnedpopcorn180{NORMAL}"
		+ $"\nand {BROWN}DogMatt{NORMAL}"

		+ $"\n\n{RED}Note that this Decompiler tries its best to recreate the original project file{NORMAL}"
		+ $"\n{RED}However, it won't give you a working recreation out of the box{NORMAL}"
		+ $"\n{RED}You most likely will have to tweak things like events to get a functional recreation{NORMAL}"

		+ $"\n{GREEN}With that being said, have fun{NORMAL}"

		+ $"\n"
		);

		#region Arguments and Folders
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
            PushToConsoleLog($"ERROR: No Bank file path provided\nQuitting...", RED);
			return;
		}
		if (outputProjectPath == "")
		{
            PushToConsoleLog($"ERROR: No Output file path provided\nQuitting...", RED);
			return;
		}

		// remove any qoutes in the strings, just in case
		bankFolder = bankFolder.Replace("\"", "");
		outputProjectPath = outputProjectPath.Replace("\"", "");

		// If bank folder doesn't exist
		if (!Directory.Exists(bankFolder))
		{
			PushToConsoleLog($"ERROR: Bank Folder does not exist\nQuitting...", RED);
			return;
		}

		// If output folder doesn't exist, warn user
		if (!Directory.Exists(bankFolder))
			PushToConsoleLog($"WARNING: Output Folder does not exist\nContinuing Anyways...", YELLOW);

		#endregion

		// Get Project Name
		if (projectname == "Generic-Project" && !IsGUI)
		{
			Console.Write("Enter the Project Name: ");
			projectname = Console.ReadLine();
		}
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
        PushToConsoleLog($"Loading Banks...", YELLOW);
		// (but get Master.strings first)
        var FolderFiles = Directory.GetFiles(bankFolder, "*.bank")
			.OrderByDescending(f => Path.GetFileName(f).Equals("Master.strings.bank", StringComparison.OrdinalIgnoreCase));

        #region Idiot Proof Checks
        if (Directory.GetFiles(bankFolder, "*.fsb").Length > 0)
        {
            // prevent user from using FSB4 files, since this tool obviously doesn't support that
            PushToConsoleLog("ERROR: Input is unsupported (FSB4)\nQuitting...", RED);
            return;
        }
        if (!FolderFiles.Any())
        {
            // prevent user from using FSB4 files, since this tool obviously doesn't support that
            PushToConsoleLog("ERROR: No .bank files were found\nQuitting...", RED);
            return;
        }
        if (!File.Exists($"{bankFolder}/Master.strings.bank"))
        {
            PushToConsoleLog("ERROR: Master.strings.bank is not present\nQuitting...", RED);
            return;
        }
        #endregion

		// go through all bank files in folder
        foreach (string bankFilePath in FolderFiles)
		{
            var bank = FModBankParser.FModBankParser.LoadSoundBank(new FileInfo(bankFilePath));
			string bankName = bank.BankName;
			FModGuid bankGuid = bank.BankInfo.BaseGuid;

            PushToConsoleLog($"Loaded Bank: {bankName} (GUID: {bankGuid})", GREEN);
            PushToConsoleLog($"Bank Version: {bank.BankInfo.FileVersion}", GREEN);

			if (bankName == "Master.strings.bank")
			{
				StringTable = bank.StringTable.RadixTree;
				continue;
			}

			// Spinner for when --verbose was not used
			if (!verbose)
				StartSpinnerAsync("Extracting Bank Info...", new Random().Next(2), 1000, SpinnerKill.Token);

			// Start actual extraction

            // basically just the XML Files for most assets that references their given bank file
            // Master.bank has already been added, so skip it
            #region Bank Specific XMLs
            if (bankName != "Master.bank")
            {
                MasterXMLs.Create_BankAssetXML(bankName.Replace(".bank", "/"));
                MasterXMLs.Create_BankFileXML(bankGuid, bank.BankName.Replace(".bank", ""));
            }
            #endregion
            #region Event stuff
            PushToConsoleLog($"Event Count: {bank.EventNodes.Count}", GREEN);

			// first get event folders
			foreach (FModGuid eGuid in bank.EventNodes.Keys)
			{
				EventNode Event = bank.EventNodes[eGuid];
				// TODO
            }

			// after event folders, do actual events
            foreach (FModGuid eGuid in bank.EventNodes.Keys)
            {
                EventNode Event = bank.EventNodes[eGuid];

				if (!StringTable.TryGetString(Event.BaseGuid, out string EventPath))
					Events.EventXML(Event, EventPath, bank);
				else { }//TODO
            }
			#endregion

			// Export all Sounds
			foreach (FmodSoundBank sndBank in bank.SoundBankData)
                ExtractSoundAssets.ExtractSoundFiles(sndBank, bankName);
        }

		#region Finish
		// if not verbose, stop spinner
		if (!verbose)
			SpinnerKill.Cancel();

		PushToConsoleLog($"{USESPACE}\nConversion Complete!", GREEN);
		PushToConsoleLog($"Exported Project is at {outputProjectPath}", GREEN);
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
				string fullMessage = displayMsg + "	" + sequence[sequenceCode, counterValue];

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