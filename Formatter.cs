extern alias unityengineold;
using System;
using System.Collections.Generic;
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
		public const string ROACHES = "{roaches}";
		public const string ROACHES_BANKED_THIS_RUN = "{roaches_banked_this_run}";
		public static readonly string EMPTY = "{empty}";

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
					.Replace(LEVEL, currLevel.levelName)
					.Replace(BEST_LEVEL_TIME, Math.Round(Timer.bestLevelTime(currLevel), 2).ToString());
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
				.Replace(LEVEL_TIME, Math.Round(Timer.currentLevelTime(), 2).ToString())
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
				.Replace(ROACHES, CL_GameManager.roaches.ToString())
				.Replace(ROACHES_BANKED_THIS_RUN, Plugin.roachesBankedThisRun.ToString())
				.Replace(EMPTY, "");
		}
		catch (NullReferenceException _) { }

		return "ERROR";
	} 
}