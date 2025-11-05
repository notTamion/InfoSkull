using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_GameManager))]
public class GameManagerPatch {
	
	[HarmonyPatch("Pause")]
	[HarmonyPostfix]
	public static void postfixPause(CL_GameManager __instance) { 
		var reportBug = GameObject.Find("Report Bug");
		if (reportBug && !GameObject.Find("Adjust UI")) {
			var adjustUI = GameObject.Instantiate(reportBug, new Vector3(Screen.width - reportBug.transform.position.x, reportBug.transform.position.y, 0), Quaternion.identity,
				GameObject.Find("Pause Buttons").transform);
			adjustUI.name = "Adjust UI";
			var button = adjustUI.GetComponent<Button>();
			button.name = "Adjust UI";
			adjustUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Adjust UI");
			button.onClick.AddListener(() => {
				core.InfoSkull.openAdjustUI();
			});
			button.enabled = true;
			adjustUI.GetComponent<UI_AnimateOnSelect>().enabled = true;
			adjustUI.GetComponent<UI_MenuButton>().enabled = true;
		}
	}
	
	[HarmonyPatch("UnPause")]
	[HarmonyPostfix]
	public static void postfixUnPause(CL_GameManager __instance) {
		if (core.InfoSkull.isAdjustingUI) {
			core.InfoSkull.disableAdjustUI();
		}
	}
}