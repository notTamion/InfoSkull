using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_GameManager))]
public class GameManagerPatch {
	[HarmonyPatch("SetGameTime")]
	[HarmonyPostfix]
	public static void postfixSetGameTime(CL_GameManager __instance) {
		Timer.levelEnterTime = __instance.GetGameTime();
	}

	[HarmonyPatch(MethodType.Constructor)]
	[HarmonyPostfix]
	public static void postfixConstructor(CL_GameManager __instance) {
		Timer.levelEnterTime = __instance.GetGameTime();
	}
}