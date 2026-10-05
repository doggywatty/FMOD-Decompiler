using Fmod5Sharp.FmodTypes;
using FModBankParser.Nodes;
using FModBankParser.Nodes.Buses;
using FModBankParser.Nodes.Instruments;
using FModBankParser.Objects;
using System.Globalization;
using System.Runtime.InteropServices;

public class Program
{
    #region Colored Text
	// thank you https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
	public static string SPACE = "\r											"; // shortcut for when not verbose
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

	public static Guid ParameterPresetFolderGUID = GetRandomGUID();

	public static Guid MasterXMLGUID = GetRandomGUID(); // for Master.XML
	public static Guid MasterMixerXMLGUID = GetRandomGUID(); // for Mixer.XML
	public static Guid EventOutputBusGUID = GetRandomGUID(); // catch-all MixerGroup every event routes into
	public static Guid MasterTagsXMLGUID = GetRandomGUID(); // for Tags.XML
	public static Guid MasterWorkspaceXMLGUID = GetRandomGUID(); // for Workspace.XML

	public static Guid Master1GUID = GetRandomGUID();// for Master.XML (effectChain)
	public static Guid Master2GUID = GetRandomGUID();// for Master.XML (panner)
	public static Guid Master3GUID = GetRandomGUID();// for Master.XML (effect)

	// these keep track of all randomly generated GUIDs, so we can call them back if needed elsewhere
	public static Dictionary<string, Guid> EventFolderGUIDs = [];
	public static Dictionary<string, Guid> AudioFileGUIDs = [];
	public static Dictionary<FModGuid, Guid> WavGUIDs = [];
	public static Dictionary<FModGuid, BaseInstrumentNode> AllInstrumentNodes = [];
	
	// Maps ParameterNode BaseGuid to ParameterPreset object GUID (for ParameterProxy.preset links)
	public static Dictionary<FModGuid, Guid> ParameterPresetGUIDs = [];
	
	// Tracks already emitted ParameterPresets so duplicate ParameterNodes across banks don't double-write
	public static HashSet<string> EmittedParameterPresets = [];
	
	// Tracks already emitted Bank GUIDs (localized banks share GUIDs, just emit once)
	public static HashSet<string> EmittedBankGUIDs = [];

	/*
		Every bus seen across all banks, plus the bus GUIDs that events already claim (EventMixerMaster / EventMixerInput / EventMixerGroup)
		Whatever is left over is the standalone mixer tree and gets emitted as MixerGroup / MixerReturn, which snapshots animate directly
	*/
	public static Dictionary<string, BaseBusNode> AllBusNodes = [];
	public static HashSet<string> ReferencedBusGuids = [];
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
			File.AppendAllText($"{outputProjectPath}/log.txt", "\n" + message);
 
	}

	// Create Random GUIDs
	public static Guid GetRandomGUID() => Guid.NewGuid();

	// because some things need to be converted like this
	public static double GetValue(long val) => val / 48000.0;

	public static List<string> SplitEventPath(string EventPath)
	{
		// Get the subfolders by splitting by "/"
		List<string> folders = [.. EventPath.Split('/')];
		folders.RemoveAt(0); // Remove "event:"
		folders.RemoveAt(folders.Count - 1); // Remove the last part (it's not a folder)
		return folders;
	}
	#endregion

	public static async Task Main(string[] args)
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
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
			try { Console.Clear(); } catch { }
		}

		Console.WriteLine($"Welcome to the FMOD Bank Decompiler {GREEN}(Version 2.0.0 DEV){NORMAL}"
		+ $"\n\nby {OTHERGRAY}burnedpopcorn180{NORMAL}"
		+ $"\nand {BROWN}DogMatt{NORMAL}"

		+ $"\n\n{RED}Note that this Decompiler tries its best to recreate the original project file{NORMAL}"
		+ $"\n{RED}However, it won't give you a working recreation out of the box{NORMAL}"
		+ $"\n{RED}You most likely will have to tweak things like events to get a functional recreation{NORMAL}"

		+ $"\n{GREEN}With that being said, have fun{NORMAL}"

		+ $"\n"
		);

		#region Arguments and Folders
		// if no arguments were added, prompt interactively
		if (args.Length == 0)
		{
			Console.Write("Enter the path to the Bank Folder: ");
			bankFolder = Console.ReadLine() ?? "";
			Console.Write("Enter the path to output the FSPRO Project: ");
			outputProjectPath = Console.ReadLine() ?? "";
		}
		else if (bankFolder == "" || outputProjectPath == "")
		{
			PushToConsoleLog("ERROR: Missing required arguments. Use --input <path> --output <path>", RED);
			return;
		}

		bankFolder = bankFolder.Replace("\"", "").Trim();
		outputProjectPath = outputProjectPath.Replace("\"", "").Trim();

		if (string.IsNullOrWhiteSpace(bankFolder))
		{
			PushToConsoleLog($"ERROR: No Bank file path provided\nQuitting...", RED);
			return;
		}
		if (string.IsNullOrWhiteSpace(outputProjectPath))
		{
			PushToConsoleLog($"ERROR: No Output file path provided\nQuitting...", RED);
			return;
		}

		// directory validation
		if (!Directory.Exists(bankFolder))
		{
			PushToConsoleLog($"ERROR: Bank Folder does not exist: {bankFolder}\nQuitting...", RED);
			return;
		}
		if (!Directory.Exists(outputProjectPath))
			PushToConsoleLog("WARNING: Output Folder does not exist\nContinuing Anyways...", YELLOW);

		#endregion

		// Get Project Name
		if (projectname == "Generic-Project" && !IsGUI)
		{
			Console.Write("Enter the Project Name: ");
			projectname = Console.ReadLine() ?? "";
		}
		var USESPACE = !verbose ? SPACE : "";

		if (string.IsNullOrEmpty(projectname))
			projectname = "Generic-Project";

		#region Setup Output Folders

		// ensure output directory exists and is clean
		if (Directory.Exists(outputProjectPath))
		{
			PushToConsoleLog($"WARNING: Output folder exists, overwriting: {outputProjectPath}", YELLOW);
			Directory.Delete(outputProjectPath, true);
		}
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
		Directory.CreateDirectory(outputProjectPath + "/Metadata/Snapshot");
		Directory.CreateDirectory(outputProjectPath + "/Metadata/SnapshotGroup");
		Directory.CreateDirectory(outputProjectPath + "/Metadata/Event");
		// Standalone mixer tree
		Directory.CreateDirectory(outputProjectPath + "/Metadata/Group");
		Directory.CreateDirectory(outputProjectPath + "/Metadata/Return");
		Directory.CreateDirectory(outputProjectPath + "/Metadata/VCA");

		#endregion
		#region Built-in XML Files
		// this is basically just stuff that is ALWAYS gonna be in a FSPro Project

		// XML Files that are in their own subfolders
		MasterXMLs.Create_MasterAssetXML();
		MasterXMLs.Create_MasterBankFoldersXML();
		MasterXMLs.Create_EventFolderXML();
		MasterXMLs.Create_PlatformXML();
		MasterXMLs.Create_EncodingSettingXML();
		MasterXMLs.Create_EffectPresetFolderXML();
		MasterXMLs.Create_ParameterPresetFolderXML();
		MasterXMLs.Create_ProfilerFolderXML();
		MasterXMLs.Create_SandboxFolderXML();

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
			string rawBankName = bank.BankName;
			FModGuid bankGuid = bank.BankInfo.BaseGuid;

			PushToConsoleLog($"Loaded Bank: {rawBankName} (GUID: {bankGuid})", GREEN);
			PushToConsoleLog($"Bank Version: {bank.BankInfo.FileVersion}", GREEN);

			// normalize bank name, strip .bank extension if present
			string bankName = rawBankName.EndsWith(".bank", StringComparison.OrdinalIgnoreCase) ? rawBankName[..^5] : rawBankName;

			if (string.Equals(bankName, "Master.strings", StringComparison.OrdinalIgnoreCase))
			{
				if (bank.StringTable?.RadixTree is not null)
					StringTable = bank.StringTable.RadixTree;
				continue;
			}

			if (!verbose)
			{
				#pragma warning disable CS4014
				StartSpinnerAsync("Extracting Bank Info...", new Random().Next(2), 1000, SpinnerKill.Token);
				#pragma warning restore CS4014
			}

			#region Setup WavEntries
			foreach (FModGuid WavGuid in bank.WavEntries.Keys) 
			{
				WaveformResourceNode Wav = bank.WavEntries[WavGuid];
				string? WavSampleName =
					bank.SoundBankData[Wav.SoundBankIndex]
					.Samples[Wav.SubsoundIndex]
					.Name;

				if (string.IsNullOrEmpty(WavSampleName)) 
					continue;

				/*
					just normalize to the extension-less name
					AudioFile.AudioFileXML derives its key from the written file name, so any difference here produces a GUID that is never written out and leaves a dangling "audioFile" reference (then studio flags the event..)
				*/
				string wavKey = Path.GetFileNameWithoutExtension(WavSampleName);
				if (string.IsNullOrEmpty(wavKey))
					wavKey = WavSampleName;

				if (!AudioFileGUIDs.TryGetValue(wavKey, out Guid SavedAudioFileGuid))
				{
					Guid AudioFileGuid = GetRandomGUID();
					AudioFileGUIDs.Add(wavKey, AudioFileGuid);
					WavGUIDs.TryAdd(WavGuid, AudioFileGuid);
				}
				else
					WavGUIDs.TryAdd(WavGuid, SavedAudioFileGuid);
			}

			/*
				Pre-populate AudioFile GUIDs from raw samples so localized/audio-table banks (Dialogue_*) with no WavEntries still get stable GUIDs and no warnings.
				Key on the extensionless name because that's what AudioFileXML looks up.
			*/
			foreach (FmodSoundBank sndBank in bank.SoundBankData)
			{
				foreach (var sample in sndBank.Samples)
				{
					string? sname = sample.Name;
					if (string.IsNullOrEmpty(sname)) continue;
					string keyBase = Path.GetFileNameWithoutExtension(sname);
					if (string.IsNullOrEmpty(keyBase)) continue;
					AudioFileGUIDs.TryAdd(keyBase, GetRandomGUID());
				}
			}

			foreach (var kvp in bank.InstrumentNodes)
				AllInstrumentNodes.TryAdd(kvp.Key, kvp.Value);

			foreach (var kvp in bank.BusNodes)
				AllBusNodes.TryAdd(kvp.Key.ToString(), kvp.Value);

			// Mark the buses this bank' events already own so they are not re-emitted as part of the standalone mixer tree.
			foreach (var ev in bank.EventNodes.Values)
			{
				if (!ev.MasterTrackGuid.IsEmpty)
					ReferencedBusGuids.Add(ev.MasterTrackGuid.ToString());
				if (!ev.InputBusGuid.IsEmpty)
					ReferencedBusGuids.Add(ev.InputBusGuid.ToString());
				if (ev.NonMasterTracks != null)
					foreach (var nmt in ev.NonMasterTracks)
						ReferencedBusGuids.Add(nmt.ToString());
			}

			/*
				A bank with sounds but no events/instruments is an audio table (loose files).
				Localized variants (for example, Dialogue_CN/EN/JP) share a GUID, so the audio table source dir is the shared prefix without the language suffix
			*/
			bool isAudioTable = bank.EventNodes.Count == 0 && bank.InstrumentNodes.Count == 0 && bank.SoundBankData.Count > 0;
			string? tableDir = null;
			if (isAudioTable)
			{
				int lastUnderscore = bankName.LastIndexOf('_');
				tableDir = lastUnderscore > 0 ? bankName[..lastUnderscore] : bankName;
			}

			foreach (FmodSoundBank sndBank in bank.SoundBankData)
				ExtractSoundAssets.ExtractSoundFiles(sndBank, bankName, isAudioTable, tableDir);
			#endregion

			#region Bank Specific XMLs
			bool isMaster = string.Equals(bankName, "Master", StringComparison.OrdinalIgnoreCase);
			string bankAssetPath = bankName + "/";
			// localized banks (Dialogue_CN/EN/JP) share one Bank GUID, only emit the Bank XML once.
			bool firstTimeForGuid = EmittedBankGUIDs.Add(bankGuid.ToString());
			if (firstTimeForGuid)
				MasterXMLs.Create_BankFileXML(bankGuid, isAudioTable ? tableDir ?? bankName : bankName, isMaster, isAudioTable, tableDir);
			// Audio tables hold loose files, so they don't own an EncodableAsset.
			if (!isAudioTable)
				MasterXMLs.Create_BankAssetXML(bankAssetPath);
			#endregion

			// Parameters FIRST so ParameterPresetGUIDs map is ready for ParameterProxy.preset links
			foreach (FModGuid p in bank.ParameterNodes.Keys)
				Parameters.ParameterXML(bank.ParameterNodes[p]);

			#region Event Stuff
			PushToConsoleLog($"Event Count: {bank.EventNodes.Count}", GREEN);

			// Track which SnapshotNodes were consumed via snapshot-events, so the legacy fallback below doesn't duplicate them.
			HashSet<string> consumedSnapshotNodes = new();

			foreach (FModGuid eGuid in bank.EventNodes.Keys)
			{
				EventNode Event = bank.EventNodes[eGuid];

				if (StringTable is not null && StringTable.TryGetString(Event.BaseGuid, out string EventPath))
				{
					// Banks store Snapshots as EventNodes with snapshot:/ paths, route them to Snapshot handling, not Event handling
					if (EventPath.StartsWith("snapshot:/", StringComparison.OrdinalIgnoreCase))
					{
						bank.SnapshotNodes.TryGetValue(Event.SnapshotGuid, out SnapshotNode? snapData);
						if (snapData is not null)
							consumedSnapshotNodes.Add(Event.SnapshotGuid.ToString());
						Snapshots.SnapshotXML(Event, snapData, EventPath);
						continue;
					}

					// Create XMLs for the Event Path (if they don't exist already)
					EventFolder.ExtractEventFolders(EventPath);
					// Create XML for the event
					Events.EventXML(Event, EventPath, bank);
				}
				else
				{
					PushToConsoleLog($"WARNING: Event Path could not be resolved for Event ID ({Event.BaseGuid}); using fallback name", YELLOW);
					string fallbackName = $"Event-{Event.BaseGuid.ToString()[..8]}";
					string fallbackPath = $"event:/{fallbackName}";
					EventFolder.ExtractEventFolders(fallbackPath);
					Events.EventXML(Event, fallbackPath, bank);
				}
			}
			#endregion
			#region Snapshots
			foreach (FModGuid s in bank.SnapshotNodes.Keys)
			{
				if (consumedSnapshotNodes.Contains(s.ToString()))
					continue;
				// Fallback for SnapshotNodes with no matching snapshot-event (like said before, it should be rare, again)
				SnapshotNode Snap = bank.SnapshotNodes[s];
				PushToConsoleLog($"WARNING: SnapshotNode {s} has no matching snapshot-event, using fallback", YELLOW);
				Snapshots.SnapshotXML_Legacy(Snap);
			}
			#endregion
		}

		#region Finalize Global Metadata
		Snapshots.SnapshotGroupXML();
		// Emit the standalone mixer buses (snapshots animate them by GUID)
		MasterXMLs.Create_StandaloneMixerBuses();
		// Create the FSPro file LAST so it can include all object references
		MasterXMLs.Create_FSPROFile(projectname);
		#endregion

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
				Console.Write("\r													");

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