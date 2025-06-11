using HarmonyLib;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(CL_SaveManager))]
public class SaveManagerPatch {
	[HarmonyPatch("LoadSession")]
	[HarmonyPostfix]
	static void postfixLoadSession() {
		if (CL_GameManager.HasActiveFlag("leaderboardIllegal")) {
			
			CL_GameManager.gamemode.allowLeaderboardScoring = false;
		}
		Plugin.checkLeaderboardLegality();
	}
}