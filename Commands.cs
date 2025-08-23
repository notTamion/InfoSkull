extern alias unityengineold;
using System.Linq;
using System.Reflection;
using InfoSkull.utils;

namespace InfoSkull;

/// <summary>
/// Handles command registration and execution for the InfoSkull plugin.
/// Provides console commands for configuring level timers, display formats, and managing presets.
/// </summary>
public class Commands {
	/// <summary>
	/// Reference to the game's command console instance for sending messages.
	/// </summary>
	public static CommandConsole console;

	/// <summary>
	/// Registers all InfoSkull commands with the game's command console.
	/// Commands are accessible via the "is" prefix (e.g., "is leveltimer").
	/// </summary>
	public static void register() {
		console.RegisterCommand("is", args => {
			if (args.Length == 0) args = new [] {""};
			switch (args[0]) {
				case "leveltimer":
					levelTimer(args.Skip(1).ToArray());
					break;
				case "display":
					display(args.Skip(1).ToArray());
					break;
				case "preset":
					preset(args.Skip(1).ToArray());
					break;
				case "wipe":
					StatsUtil.wipe(pair => pair.Key.StartsWith("info-skull-"));
					sendMessage("Wiped all data");
					break;
				#if DEBUG
				case "dev":
					dev(args.Skip(1).ToArray());
					break;
				#endif
				default:
					sendMessage(@" 
					            Commands:
					            leveltimer: level timer and end of level text 
					            display: Text that's displayed at the top of the screen
					            preset: export/import/save/load/list/delete presets
					            preset file-save <name>: save preset JSON under plugins folder
					            preset file-load <name>: load preset JSON from plugins folder
					            preset file-list: list JSON presets
					            preset file-delete <name>: delete JSON preset
					            wipe: wipes the data saved (doesn't include config). BACKUP YOUR SAVE FILE BEFORE DOING THIS!
					            ");
					break;
			}
		}, false);
	}

	/// <summary>
	/// Handles level timer related commands for configuring end-of-level display and timing behavior.
	/// </summary>
	/// <param name="args">Command arguments array</param>
	static void levelTimer(string[] args) {
		if (args.Length == 0) args = new [] {""};
		switch (args[0]) {
			case "format":
				ConfigService.Data.levelTimerFormat = string.Join(" ", args.Skip(1).ToArray()).Replace("\\n", "\n");
				ConfigService.Save();
				sendMessage("Updated format");
				Plugin.checkLeaderboardLegality();
				break;
			case "onlybest":
				ConfigService.Data.levelTimerOnlyBest = !ConfigService.Data.levelTimerOnlyBest;
				ConfigService.Save();
				sendMessage("Now " + (ConfigService.Data.levelTimerOnlyBest ? "only new best" : "all") +
				            " times will be shown");
				break;
			case "saving":
				ConfigService.Data.levelTimerSaving = !ConfigService.Data.levelTimerSaving;
				ConfigService.Save();
				sendMessage("Best times will " + (ConfigService.Data.levelTimerSaving ? "now" : "no longer") + " be saved");
				break;
			case "wipe":
				StatsUtil.wipe(pair => pair.Key.StartsWith("info-skull-") && pair.Key.EndsWith("-best-time"));
				sendMessage("Wiped all best times data");
				break;
			default:
				sendMessage(@" 
				            Commands:
				            format: Sets the format for the end of level message. See Github for possible formats
				            onlybest: toggles whether the text only appears on new PBs
				            wipe: wipes the data saved for level times (doesn't include config). BACKUP YOUR SAVE FILE BEFORE DOING THIS!
				            ");
				break;
		}
	}

	/// <summary>
	/// Handles display-related commands for configuring on-screen text display format.
	/// </summary>
	/// <param name="args">Command arguments array</param>
	static void display(string[] args) {
		if (args.Length == 0) args = new [] {""};
		switch (args[0]) {
			case "format":
				ConfigService.Data.displayFormat = string.Join(" ", args.Skip(1).ToArray()).Replace("\\n", "\n");
				ConfigService.Save();
				sendMessage("Updated format");
				Plugin.checkLeaderboardLegality();
				break;
			default:
				sendMessage(@" 
				            Commands:
				            format: Sets the format for the display. See Github for possible formats
				            ");
				break;
		}
	}

	/// <summary>
	/// Handles preset-related commands for saving, loading, listing, and deleting configuration presets.
	/// </summary>
	/// <param name="args">Command arguments array</param>
	static void preset(string[] args) {
		if (args.Length == 0) args = new [] {""};
		switch (args[0]) {
			case "file-save": {
				var name = string.Join(" ", args.Skip(1).ToArray());
				if (string.IsNullOrWhiteSpace(name)) { sendMessage("Usage: is preset file-save <name>"); break; }
				ConfigService.SaveCurrentAsPresetFile(name);
				sendMessage("Saved preset JSON '" + name + "' to " + ConfigService.GetPresetsDirectory());
				break;
			}
			case "file-load": {
				var name = string.Join(" ", args.Skip(1).ToArray());
				var preset = ConfigService.LoadPresetFromFile(name);
				if (preset == null) { sendMessage("Preset JSON not found: '" + name + "'"); break; }
				ConfigService.ApplyPreset(preset);
				Plugin.checkLeaderboardLegality();
				var method = typeof(Plugin).GetMethod("RebuildUI", BindingFlags.NonPublic | BindingFlags.Static);
				method?.Invoke(null, null);
				sendMessage("Loaded preset JSON '" + name + "'");
				break;
			}
			case "file-list": {
				var names = ConfigService.ListPresetNames();
				sendMessage(names.Length == 0 ? "No JSON presets" : ("JSON presets: " + string.Join(", ", names)));
				break;
			}
			case "file-delete": {
				var name = string.Join(" ", args.Skip(1).ToArray());
				var ok = ConfigService.DeletePresetFile(name);
				sendMessage(ok ? ("Deleted preset JSON '" + name + "'") : ("Preset JSON not found: '" + name + "'"));
				break;
			}
			default:
				sendMessage(@" 
				            Preset commands:
				            file-save <name>: save preset JSON under plugins folder
				            file-load <name>: load preset JSON from plugins folder
				            file-list: list JSON presets
				            file-delete <name>: delete JSON preset
				            ");
				break;
		}
	}

#if DEBUG
	/// <summary>
	/// Development-only commands for debugging and testing purposes.
	/// Only available when compiled in DEBUG configuration.
	/// </summary>
	/// <param name="args">Command arguments array</param>
	static void dev(string[] args) {
		switch (args[0]) {
			case "wss":
				StatsUtil.wipe(pair => pair.Key.StartsWith(args[1]));
				sendMessage($"Wiped stats that start with {args[1]}");
				break;
			case "wse":
				StatsUtil.wipe(pair => pair.Key.EndsWith(args[1]));
				sendMessage($"Wiped stats that ends with {args[1]}");
				break;
			default:
				sendMessage("Commands: wss, wse");
				break;
		}
	}
#endif

	/// <summary>
	/// Sends a message to the game's command console using reflection to access internal methods.
	/// </summary>
	/// <param name="message">The message text to display in the console</param>
	public static void sendMessage(string message) {
		if (!console) return;

		// Use reflection to access the internal AddMessageToHistory method
		// This is necessary because the method is not exposed publicly by the game
		var methodInfo = console.GetType()
			.GetMethod("AddMessageToHistory", BindingFlags.NonPublic | BindingFlags.Instance);
		methodInfo.Invoke(console, new object[] { message });
	}
}