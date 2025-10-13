extern alias unityengineold;
using System;
using System.Collections.Generic;
using HarmonyLib;
using unityengineold::UnityEngine;

namespace InfoSkull;

public class Formatter {
	public const string LEVEL = "{level}"; // level name
	public const string LEVEL_TIME = "{level_time}"; // level time
	public const string HEIGHT = "{height}"; // player height
	public const string BEST_LEVEL_TIME = "{best_level_time}"; // Best level time
	public const string ASCENT_RATE = "{ascent_rate}"; // ascent rate
	public const string GAME_TIME = "{game_time}"; // time in seconds since start of run
	public const string CLOCK = "{clock}"; // clock displaying the time
	public const string LEFT_STAMINA = "{left_stamina}"; // stamina of your left Hand. LEADERBOARD ILLEGAL
	public const string RIGHT_STAMINA = "{right_stamina}"; // stamina of your right Hand. LEADERBOARD ILLEGAL
	public const string MASS_HEIGHT = "{mass_height}"; // mass height. LEADERBOARD ILLEGAL
	public const string MASS_SPEED = "{mass_speed}"; // mass speed. LEADERBOARD ILLEGAL
	public const string MASS_ACC_MULT = "{mass_acc_mult}"; // mass acceleration multiplier
	public const string MASS_DISTANCE = "{mass_distance}"; // distance from mass to player. LEADERBOARD ILLEGAL
	public const string SCORE = "{score}"; // score 
	public const string HIGH_SCORE = "{high_score}"; // high score
	public const string ASCENT = "{ascent}"; // highest height reached in this run
	public const string VELOCITY = "{velocity}"; // your velocity
	public const string HEALTH = "{health}"; // your health. LEADERBOARD ILLEGAL
	public const string EXTRA_JUMPS = "{extra_jumps}"; // extra jumps you have remaining. LEADERBOARD ILLEGAL
	
	public static readonly string EMPTY = "{empty}"; // highest height reached in this run
	
	public static M_Level levelOverride;

	public static string format(string format, Dictionary<string, string> overrides = null) {
		try {
			if (overrides != null)
				foreach (var change in overrides)
					format = format.Replace(change.Key, change.Value);

			var player = ENT_Player.playerObject;
			var gameManager = CL_GameManager.gMan;

			if (WorldLoader.instance) {
				var currLevel = levelOverride ? levelOverride : WorldLoader.instance.GetCurrentLevel().level;
				format = format
					.Replace(LEVEL, currLevel.levelName);
				//.Replace(BEST_LEVEL_TIME, Math.Round(Timer.bestLevelTime(currLevel), 2).ToString());
			}

			if (DEN_DeathFloor.instance) {
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

				format = format
					.Replace(MASS_SPEED, Math.Round(speed, 2).ToString())
					.Replace(MASS_DISTANCE,
						Math.Round(player.transform.position.y - deathFloor.transform.position.y, 0).ToString())
					.Replace(MASS_HEIGHT, Math.Round(deathFloor.transform.position.y, 0).ToString());
			}

			TimeSpan gameTimeTemp = TimeSpan.FromSeconds(CL_GameManager.gMan.GetGameTime());
			string gameTime = gameTimeTemp.TotalHours >= 1.0
				? gameTimeTemp.ToString("hh\\:mm\\:ss\\:ff")
				: gameTimeTemp.ToString("mm\\:ss\\:ff");
			
			return format
				.Replace("\\n", "\n")
				//.Replace(LEVEL_TIME, Math.Round(Timer.currentLevelTime(), 2).ToString())
				.Replace(ASCENT_RATE, Math.Round(gameManager.GetPlayerAscentRate(), 2).ToString())
				.Replace(GAME_TIME, gameTime)
				.Replace(CLOCK, DateTime.Now.ToString("HH:mm"))
				.Replace(LEFT_STAMINA, Math.Round(player.hands[0].gripStrength, 0).ToString())
				.Replace(RIGHT_STAMINA, Math.Round(player.hands[1].gripStrength, 0).ToString())
				.Replace(HEIGHT, Math.Round(player.transform.position.y, 0).ToString())
				.Replace(SCORE, Math.Round(gameManager.GetPlayerAscent() * gameManager.GetPlayerAscentRate(), 0).ToString())
				.Replace(HIGH_SCORE, Math.Round(Traverse.Create(gameManager).Field("previousHighScore").GetValue<float>(), 0).ToString())
				.Replace(ASCENT, Math.Round(Traverse.Create(gameManager).Field("playerAscent").GetValue<float>(), 0).ToString())
				.Replace(VELOCITY, Math.Round(Traverse.Create(player).Field("lastVel").GetValue<Vector3>().magnitude, 2).ToString())
				.Replace(EXTRA_JUMPS, Traverse.Create(player).Field("extraJumpsRemaining").GetValue<int>().ToString())
				.Replace(HEALTH, Math.Round(player.health, 1).ToString())
				.Replace(MASS_ACC_MULT, Math.Round(CL_GameManager.gamemode.gooSpeedIncreaseMult).ToString())
				.Replace(EMPTY, "");
		}
		catch (NullReferenceException _) { }

		return "ERROR";
	} 
}