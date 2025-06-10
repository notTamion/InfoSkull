using System;
using System.Collections.Generic;

namespace InfoSkull;

public class Formatter {
	public static M_Level levelOverride;

	public const string LEVEL = "{level}";
	public const string LEVEL_TIME = "{level_time}";
	public const string BEST_LEVEL_TIME = "{best_level_time}";
	public const string ASCENT_RATE = "{ascent_rate}";
	public const string GAME_TIME = "{game_time}";
	public const string CLOCK = "{clock}";
	public const string LEFT_STAMINA = "{left_stamina}";
	public const string RIGHT_STAMINA = "{right_stamina}";
	
	public static string format(string format, Dictionary<string, string> overrides = null) {
		
		if (overrides != null) {
			foreach (var change in overrides) {
				format = format.Replace(change.Key, change.Value);
			}
		}

		if (WorldLoader.instance) {
			M_Level currLevel = levelOverride ? levelOverride : WorldLoader.instance.currentLevel.level;
			levelOverride = null;
			format = format
				.Replace(LEVEL, currLevel.levelName)
				.Replace(BEST_LEVEL_TIME, Math.Round(Timer.bestLevelTime(currLevel), 2).ToString());
		}

		return format
			.Replace("\\n", "\n")
			.Replace(LEVEL_TIME, Math.Round(Timer.currentLevelTime(), 2).ToString())
			.Replace(ASCENT_RATE, Math.Round(CL_GameManager.gMan.GetPlayerAscentRate(), 2).ToString())
			.Replace(GAME_TIME, Math.Round(CL_GameManager.gMan.GetGameTime(), 2).ToString())
			.Replace(CLOCK, DateTime.Now.ToString("HH:mm"))
			.Replace(LEFT_STAMINA, Math.Round(ENT_Player.GetPlayer().hands[0].gripStrength, 0).ToString())
			.Replace(RIGHT_STAMINA, Math.Round(ENT_Player.GetPlayer().hands[1].gripStrength, 0).ToString());
	}
}