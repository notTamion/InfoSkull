using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CommandConsole))]
public class CommandConsolePatch {
	[HarmonyPatch("Awake")]
	[HarmonyPostfix]
	public static void postfixAwake(CommandConsole __instance) {
		Commands.console = __instance;
		Commands.register();
	}
}