using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(M_Level))]
public class LevelPatch {
	[HarmonyPatch("OnExit")]
	[HarmonyPostfix]
	public static void prefixOnExit(M_Level __instance) {
		var new_level = WorldLoader.GetClosestLevelToPosition(ENT_Player.GetPlayer().transform.position).level;
		if (new_level.HasEntered()) return;

		if (__instance.levelName.Equals("M1_Intro_01")) {
			TriggerZonePatch.time_on_close = __instance;
			return;
		}

		Plugin.logger.LogInfo("Changed level: " + __instance.levelName + " - " + new_level.levelName);

		Timer.completeRoom(__instance);
	}
}