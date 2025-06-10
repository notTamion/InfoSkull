extern alias unityengineold;
using System.Linq;
using System.Reflection;
using InfoSkull.utils;

namespace InfoSkull;

public class Commands {
	public static CommandConsole console;

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
					sendMessage("""
					            Commands:
					            leveltimer: level timer and end of level text 
					            display: Text that's displayed at the top of the screen
					            wipe: wipes the data saved (doesn't include config). BACKUP YOUR SAVE FILE BEFORE DOING THIS!
					            """);
					break;
			}
		}, false);
	}

	static void levelTimer(string[] args) {
		if (args.Length == 0) args = new [] {""};
		switch (args[0]) {
			case "format":
				Plugin.LevelTimer.format.Value = string.Join(" ", args.Skip(1).ToArray()).Replace("\\n", "\n");
				sendMessage("Updated format");
				break;
			case "onlybest":
				Plugin.LevelTimer.onlyBest.Value = !Plugin.LevelTimer.onlyBest.Value;
				sendMessage("Now " + (Plugin.LevelTimer.onlyBest.Value ? "only new best" : "all") +
				            " times will be shown");
				break;
			case "saving":
				Plugin.LevelTimer.saving.Value = !Plugin.LevelTimer.saving.Value;
				sendMessage("Best times will " + (Plugin.LevelTimer.saving.Value ? "now" : "no longer") + " be saved");
				break;
			case "wipe":
				StatsUtil.wipe(pair => pair.Key.StartsWith("info-skull-") && pair.Key.EndsWith("-best-time"));
				sendMessage("Wiped all best times data");
				break;
			default:
				sendMessage("""
				            Commands:
				            format: Sets the format for the end of level message. See Github for possible formats
				            onlybest: toggles whether the text only appears on new PBs
				            wipe: wipes the data saved for level times (doesn't include config). BACKUP YOUR SAVE FILE BEFORE DOING THIS!
				            """);
				break;
		}
	}

	static void display(string[] args) {
		if (args.Length == 0) args = new [] {""};
		switch (args[0]) {
			case "format":
				Plugin.Display.format.Value = string.Join(" ", args.Skip(1).ToArray()).Replace("\\n", "\n");
				sendMessage("Updated format");
				break;
			default:
				sendMessage("""
				            Commands:
				            format: Sets the format for the display. See Github for possible formats
				            """);
				break;
		}
	}

#if DEBUG
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

	static void sendMessage(string message) {
		var methodInfo = console.GetType()
			.GetMethod("AddMessageToHistory", BindingFlags.NonPublic | BindingFlags.Instance);
		methodInfo.Invoke(console, new object[] { message });
	}
}