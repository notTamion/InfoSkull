using HarmonyLib;
using InfoSkull.components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_GameManager))]
public class GameManagerPatch {
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
		Formatter.levelOverride = null;
		Plugin.checkLeaderboardLegality();
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

	[HarmonyPatch("Pause")]
	[HarmonyPostfix]
	public static void postfixPause(CL_GameManager __instance) { 
		var reportBug = GameObject.Find("Report Bug");
		if (reportBug && !GameObject.Find("Adjust UI")) {
			var adjustUI = Plugin.adjustUI = GameObject.Instantiate(reportBug, new Vector3(Screen.width - reportBug.transform.position.x, reportBug.transform.position.y, 0), Quaternion.identity,
				GameObject.Find("Pause Buttons").transform);
			adjustUI.name = "Adjust UI";
			var button = adjustUI.GetComponent<Button>();
			button.name = "Adjust UI";
			adjustUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Adjust UI");
			button.onClick.AddListener(() => {
				Plugin.enableUIAdjust();
			});
			button.enabled = true;
			adjustUI.GetComponent<UI_AnimateOnSelect>().enabled = true;
			adjustUI.GetComponent<UI_MenuButton>().enabled = true;
		}
	}
	
	[HarmonyPatch("UnPause")]
	[HarmonyPostfix]
	public static void postfixUnPause(CL_GameManager __instance) {
		if (Plugin.isAdjustingUI) {
			Plugin.disableUIAdjust();
		}
	}
}