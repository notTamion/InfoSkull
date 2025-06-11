using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace InfoSkull;

public class Formatter {
	public static M_Level levelOverride;

	public const string LEVEL = "{level}";                     // level name
	public const string LEVEL_TIME = "{level_time}";           // level time
	public const string HEIGHT = "{height}";                   // player height
	public const string BEST_LEVEL_TIME = "{best_level_time}"; // Best level time, requires level timer saving to be enabled
	public const string ASCENT_RATE = "{ascent_rate}";         // ascent rate
	public const string GAME_TIME = "{game_time}";             // time in seconds since start of run
	public const string CLOCK = "{clock}";                     // clock displaying the time
	public const string LEFT_STAMINA = "{left_stamina}";       // stamina of your left Hand
	public const string RIGHT_STAMINA = "{right_stamina}";     // stamina of your right Hand
	public const string MASS_HEIGHT = "{mass_height}";         // mass height
	public const string MASS_SPEED = "{mass_speed}";           // mass speed
	public const string MASS_ACC_MULT = "{mass_acc_mult}";     // mass acceleration multiplier
	public const string MASS_DISTANCE = "{mass_distance}";     // distance from mass to player
	
	public static string format(string format, Dictionary<string, string> overrides = null) {
		
		if (overrides != null) {
			foreach (var change in overrides) {
				format = format.Replace(change.Key, change.Value);		
			}
		}
		
		ENT_Player player = ENT_Player.playerObject;
		CL_GameManager gameManager = CL_GameManager.gMan;

		if (WorldLoader.instance) {
			M_Level currLevel = levelOverride ? levelOverride : WorldLoader.instance.currentLevel.level;
			format = format
				.Replace(LEVEL, currLevel.levelName)
				.Replace(BEST_LEVEL_TIME, Math.Round(Timer.bestLevelTime(currLevel), 2).ToString());
		}

		if (DEN_DeathFloor.instance) {
			DEN_DeathFloor deathFloor = DEN_DeathFloor.instance;
			Traverse traverse = Traverse.Create(deathFloor);
			
			float num2 = CL_GameManager.gamemode ? CL_GameManager.gamemode.gooSpeedMult : 1f;
			if (SettingsManager.settings.g_hard) num2 *= 2f;
			float num3 = traverse.Field("speedMult").GetValue<float>() * num2 * traverse.Field("speedMultFrame").GetValue<float>();
			if (WorldLoader.initialized && WorldLoader.isLoaded && WorldLoader.instance.currentLevel != null)
			{
				num3 *= WorldLoader.instance.currentLevel.level.massSpeedMult;
				if (WorldLoader.instance.currentLevel.level.subRegion != null)
					num3 *= WorldLoader.instance.currentLevel.level.subRegion.massSpeedMult;
			}
			float speed = deathFloor.speed * num3;
			
			format = format
				.Replace(MASS_SPEED, Math.Round(speed, 2).ToString())
				.Replace(MASS_DISTANCE, Math.Round(player.transform.position.y - deathFloor.transform.position.y, 0).ToString())
				.Replace(MASS_ACC_MULT, Math.Round(traverse.Field("speedIncreaseRateMultiplier")
					.GetValue<float>(), 0).ToString())
				.Replace(MASS_HEIGHT, Math.Round(deathFloor.transform.position.y, 0).ToString());
		}

		return format
			.Replace("\\n", "\n")
			.Replace(LEVEL_TIME, Math.Round(Timer.currentLevelTime(), 2).ToString())
			.Replace(ASCENT_RATE, Math.Round(gameManager.GetPlayerAscentRate(), 2).ToString())
			.Replace(GAME_TIME, Math.Round(gameManager.GetGameTime(), 2).ToString())
			.Replace(CLOCK, DateTime.Now.ToString("HH:mm"))
			.Replace(LEFT_STAMINA, Math.Round(player.hands[0].gripStrength, 0).ToString())
			.Replace(RIGHT_STAMINA, Math.Round(player.hands[1].gripStrength, 0).ToString())
			.Replace(HEIGHT, Math.Round(player.transform.position.y, 0).ToString());
	}
}