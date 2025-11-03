using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_SaveManager))]
public class SaveManagerPatch {
	[HarmonyPatch("LoadSession")]
	[HarmonyPostfix]
	static void postfixLoadSession() {
	}
}