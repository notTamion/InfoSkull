using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(M_Level))]
public class LevelPatch {
	[HarmonyPatch("OnExit")]
	[HarmonyPostfix]
	public static void prefixOnExit(M_Level __instance) {
		var new_level = WorldLoader.GetClosestLevelToPosition(ENT_Player.playerObject.transform.position).level;
		if (new_level.HasEntered()) return;

		Timer.completeRoom(__instance);
	}
}