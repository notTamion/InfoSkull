extern alias unityengineold;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using unityengineold::UnityEngine;

namespace InfoSkull;

public class Formatter {
	
	public const string LEVEL = "{level}";
	public const string LEVEL_TIME = "{level_time}";
	public const string HEIGHT = "{height}";
	public const string BEST_LEVEL_TIME = "{best_level_time}";
	public const string ASCENT_RATE = "{ascent_rate}";
	public const string GAME_TIME = "{game_time}";
	public const string CLOCK = "{clock}";
	public const string LEFT_STAMINA = "{left_stamina}";
	public const string RIGHT_STAMINA = "{right_stamina}";
	public const string MASS_HEIGHT = "{mass_height}";
	public const string MASS_SPEED = "{mass_speed}";
	public const string MASS_ACC_MULT = "{mass_acc_mult}";
	public const string MASS_DISTANCE = "{mass_distance}";
	public const string SCORE = "{score}";
	public const string HIGH_SCORE = "{high_score}";
	public const string ASCENT = "{ascent}";
	public const string VELOCITY = "{velocity}";
	public const string HEALTH = "{health}";
	public const string EXTRA_JUMPS = "{extra_jumps}";
	public const string EMPTY = "{empty}";

	public static Dictionary<string, Func<string>> replacements = new Dictionary<string, Func<string>> {
		{ LEVEL, () => WorldLoader.instance.GetCurrentLevel().level.levelName },
		{ LEVEL_TIME, () => Math.Round(Timer.currentLevelTime(), 2).ToString() },
		{ HEIGHT, () => Math.Round(ENT_Player.playerObject.transform.position.y, 0).ToString() },
		{ BEST_LEVEL_TIME, () => 
			Math.Round(Timer.bestLevelTime(WorldLoader.instance.GetCurrentLevel().level), 2).ToString()
		},
		{ ASCENT_RATE, () => Math.Round(CL_GameManager.gMan.GetPlayerAscentRate(), 2).ToString() },
		{ GAME_TIME, () => {
			TimeSpan gameTimeTemp = TimeSpan.FromSeconds(CL_GameManager.gMan.GetGameTime());
			return gameTimeTemp.TotalHours >= 1.0
				? gameTimeTemp.ToString("hh\\:mm\\:ss\\:ff")
				: gameTimeTemp.ToString("mm\\:ss\\:ff");
		} },
		{ CLOCK, () => DateTime.Now.ToString("HH:mm") },
		{ LEFT_STAMINA, () => Math.Round(ENT_Player.playerObject.hands[0].gripStrength, 0).ToString() },
		{ RIGHT_STAMINA, () => Math.Round(ENT_Player.playerObject.hands[1].gripStrength, 0).ToString() },
		{ MASS_HEIGHT, () => Math.Round(DEN_DeathFloor.instance.transform.position.y, 0).ToString() }, {
			MASS_SPEED, () => {
				var deathFloor = DEN_DeathFloor.instance;
				var traverse = Traverse.Create(deathFloor);

				var num2 = CL_GameManager.gamemode ? CL_GameManager.gamemode.gooSpeedMult : 1f;
				if (SettingsManager.settings.g_hard) num2 *= 2f;
				var num3 = traverse.Field("speedMult").GetValue<float>() * num2 *
				           traverse.Field("speedMultFrame").GetValue<float>();
				if (WorldLoader.initialized && WorldLoader.isLoaded && WorldLoader.instance.GetCurrentLevel() != null) {
					num3 *= WorldLoader.instance.GetCurrentLevel().level.massSpeedMult;
					if (WorldLoader.instance.GetCurrentLevel().level.subRegion != null)
						num3 *= WorldLoader.instance.GetCurrentLevel().level.subRegion.massSpeedMult;
				}

				var speed = deathFloor.speed * num3;
				return Math.Round(speed, 2).ToString();
			}
		},
		{ MASS_ACC_MULT, () => Math.Round(CL_GameManager.gamemode.gooSpeedIncreaseMult).ToString() }, {
			MASS_DISTANCE,
			() => Math.Round(
					ENT_Player.playerObject.transform.position.y - DEN_DeathFloor.instance.transform.position.y, 0)
				.ToString()
		},
		{ SCORE, () => Math.Round(CL_GameManager.gMan.GetPlayerAscent() * CL_GameManager.gMan.GetPlayerAscentRate(), 0).ToString() },
		{ HIGH_SCORE, () => Math.Round(Traverse.Create(CL_GameManager.gMan).Field("previousHighScore").GetValue<float>(), 0).ToString() },
		{ ASCENT, () => Math.Round(Traverse.Create(CL_GameManager.gMan).Field("playerAscent").GetValue<float>(), 0).ToString() },
		{ VELOCITY, () => Math.Round(Traverse.Create(ENT_Player.playerObject).Field("lastVel").GetValue<Vector3>().magnitude, 2).ToString() },
		{ HEALTH, () => Math.Round(ENT_Player.playerObject.health, 1).ToString() },
		{ EXTRA_JUMPS, () => Traverse.Create(ENT_Player.playerObject).Field("extraJumpsRemaining").GetValue<int>().ToString() },
		{ EMPTY, () => "" }
	};
	
	public static List<string> LEADERBOARD_ILLEGAL = [
		LEFT_STAMINA,
		RIGHT_STAMINA,
		MASS_DISTANCE,
		MASS_HEIGHT,
		MASS_SPEED,
		HEALTH,
		EXTRA_JUMPS
	];
	
	public static string format(string format, Dictionary<string, string> overrides = null) { 
		if (overrides != null) {
			foreach (var change in overrides)
				format = format.Replace(change.Key, change.Value);
		}

		return Regex.Replace(format.Replace("\\n", "\n"), "{[^}]+}", match => {
			try {
				if (replacements.TryGetValue(match.Value, out var func)) {
					return func();
				}
			} catch (Exception e) {
#if DEBUG
				InfoSkullPlugin.logger.LogError(e);
#endif
				return "ERR";
			}
			return match.Value;
		});
	} 
}