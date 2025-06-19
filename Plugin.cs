using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Febucci.UI;
using Febucci.UI.Core;
using Febucci.UI.Effects;
using HarmonyLib;
using InfoSkull.components;
using InfoSkull.patches;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InfoSkull;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
	const string GUID = "InfoSkull";
	const string NAME = "InfoSkull";
	const string VERSION = "1.0.1";

	public static Plugin instance;

	internal static ManualLogSource logger;
	static Harmony harmony;

	static readonly List<string> LEADERBOARD_ILLEGAL = [
		Formatter.LEFT_STAMINA,
		Formatter.RIGHT_STAMINA,
		Formatter.MASS_DISTANCE,
		Formatter.MASS_HEIGHT,
		Formatter.MASS_SPEED,
		Formatter.HEALTH,
		Formatter.EXTRA_JUMPS
	];

	static readonly List<string> UI_POS = [
		"High Score",
		"Ascent Header",
		"Tip Header",
		"Header Text"
	];

	public static bool isAdjustingUI;
	public static GameObject adjustUI;

	public static GameObject display;
	public static GameObject levelTimer;

	public static void checkLeaderboardLegality() {
		CL_GameManager.gamemode.allowLeaderboardScoring = !LEADERBOARD_ILLEGAL.Any(illegal =>
				LevelTimer.format.Value.Contains(illegal) ||
				Display.format.Value.Contains(illegal)) && CL_GameManager.gamemode.allowLeaderboardScoring
				                                        && !CL_GameManager.HasActiveFlag("leaderboardIllegal");

		if (!CL_GameManager.gamemode.allowLeaderboardScoring) {
			GameManagerPatch.highScoreQueue = "RUN IS LEADERBOARD ILLEGAL";
			Commands.sendMessage("Leaderboard illegal variable used run will not be scored");
			CL_GameManager.SetGameFlag("leaderboardIllegal", true);
		}
	}

	void Awake() {
		instance = this;

		LevelTimer.format = Config.Bind("LevelTimer", "format", $"{Formatter.LEVEL_TIME} / {Formatter.BEST_LEVEL_TIME}",
			"Format to display");
		LevelTimer.onlyBest = Config.Bind("LevelTimer", "only-best", false, "Whether to only show new bests");
		LevelTimer.saving =
			Config.Bind("LevelTimer", "saving", true, "Whether to save best times in the games stats file");

		Display.format = Config.Bind("Display", "format", $"{Formatter.CLOCK}", "Format to display");

		logger = Logger;
		harmony = new Harmony(GUID);

		harmony.PatchAll();

		callbacks();

		logger.LogInfo($"{NAME} is loaded!");
	}

	void callbacks() {
		SceneManager.sceneLoaded += (scene, mode) => {
			var highScore = GameObject.Find("High Score");
			if (highScore) {
				Vector2 vec = highScore.GetComponent<RectTransform>().anchoredPosition;
				Display.position = Config.Bind("Display", "position", new Vector2(0, Screen.height / 2f - 50), "position to show at");
				display = GameObject.Instantiate(highScore, GameObject.Find("Game UI").transform);
				display.AddComponent<components.Display>();

				LevelTimer.position = Config.Bind("LevelTimer", "position", new Vector2(vec.x, vec.y-50), "position to show at");
				levelTimer = GameObject.Instantiate(highScore, GameObject.Find("Game UI").transform);
				levelTimer.AddComponent<components.LevelTimer>();
				
				foreach (var ui in UI_POS) {
					var gameObject = GameObject.Find(ui);
					var rectTransform = gameObject.GetComponent<RectTransform>();
					if (ui == "Ascent Header") {
						rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
						rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, Screen.height + rectTransform.anchoredPosition.y);
					}
					var config = Config.Bind(ui, "position", rectTransform.anchoredPosition, "position to show at");
					rectTransform.anchoredPosition = config.Value;
					gameObject.AddComponent<Positionable>();
				}
			}
		};
	}

	public static void enableUIAdjust() {
		GameObject.Find("Pause Menu").SetActive(false);
		isAdjustingUI = true;
		
		var displayInput = display.GetComponent<TMP_InputField>();
		displayInput.text = Display.format.Value;
		displayInput.enabled = true;
		
		var levelTimerInput = levelTimer.GetComponent<TMP_InputField>();
		levelTimerInput.text = LevelTimer.format.Value;
		levelTimerInput.enabled = true;
		
		foreach (var ui in UI_POS) {
			var gameObject = GameObject.Find(ui);
			gameObject.GetComponent<TextMeshProUGUI>().SetText(ui);
		}
	}

	public static void disableUIAdjust() {
		display.GetComponent<TMP_InputField>().enabled = false;
		
		Display.format.Value = display.GetComponent<Formatable>().lastCommittedText;
		Display.position.Value = display.GetComponent<RectTransform>().anchoredPosition;
		
		var t = levelTimer.GetComponent<TMP_InputField>();
		t.text = "";
		t.enabled = false;
		
		LevelTimer.format.Value = levelTimer.GetComponent<Formatable>().lastCommittedText;
		LevelTimer.position.Value = levelTimer.GetComponent<RectTransform>().anchoredPosition;
		
		isAdjustingUI = false;
		foreach (var ui in UI_POS) {
			var gameObject = GameObject.Find(ui);
			ConfigEntry<Vector2> s;
			instance.Config.TryGetEntry(ui, "position", out s);
			s.Value = gameObject.GetComponent<RectTransform>().anchoredPosition;
			gameObject.GetComponent<TextMeshProUGUI>().SetText("");
		}
		
		checkLeaderboardLegality();
	}
	
	public static class LevelTimer {
		public static ConfigEntry<string> format;
		public static ConfigEntry<Vector2> position;
		public static ConfigEntry<bool> onlyBest;
		public static ConfigEntry<bool> saving;
	}

	public static class Display {
		public static ConfigEntry<string> format;
		public static ConfigEntry<Vector2> position;
	}
}