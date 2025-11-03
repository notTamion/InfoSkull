using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_GameManager))]
public class GameManagerPatchBuiltin {
	public static string highScoreQueue;

	[HarmonyPatch("SetGameTime")]
	[HarmonyPostfix]
	public static void postfixSetGameTime(CL_GameManager __instance) {
		Timer.levelEnterTime = __instance.GetGameTime();
	}

	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	public static void postfixStart(CL_GameManager __instance) {
		Timer.levelEnterTime = __instance.GetGameTime();
	}

	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	public static void postfixUpdate(CL_GameManager __instance) {
		if (highScoreQueue != null && !Traverse.Create(__instance).Field("loading").GetValue<bool>() &&
		    __instance.uiMan) {
			__instance.uiMan.highscoreHeader.ShowText(highScoreQueue);
			highScoreQueue = null;
		}
	}
}
