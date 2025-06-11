using System;
using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(UT_TriggerZone))]
public class TriggerZonePatch {
	public static M_Level time_on_close;

	[HarmonyPatch("OnTriggerEnter")]
	[HarmonyPostfix]
	public static void postfixOnTriggerEnter(UT_TriggerZone __instance) {
		try {
			var new_level = (M_Level)__instance.gameObject.transform.parent.parent.GetComponent(typeof(M_Level));
			if (WorldLoader.instance.currentLevel.level != new_level || time_on_close == null ||
			    !Traverse.Create(__instance).Field<bool>("hasRun").Value) return;

			Plugin.logger.LogInfo("Changed level: " + time_on_close.levelName + " - " + new_level.levelName);
			Timer.completeRoom(time_on_close);
			Formatter.levelOverride = null;
			time_on_close = null;
		}
		catch (NullReferenceException _) { }
	}
}