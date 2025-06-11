using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Febucci.UI;
using Febucci.UI.Effects;
using HarmonyLib;
using InfoSkull.patches;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfoSkull;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
	const string GUID = "InfoSkull";
	const string NAME = "InfoSkull";
	const string VERSION = "0.2.0";

	public static Plugin instance;

	internal static ManualLogSource logger;
	static Harmony harmony;

	static readonly List<string> LEADERBOARD_ILLEGAL = [
		Formatter.LEFT_STAMINA,
		Formatter.RIGHT_STAMINA,
		Formatter.MASS_DISTANCE,
		Formatter.MASS_HEIGHT,
		Formatter.MASS_SPEED
	];

	GameObject display;

	GameObject displayPrefab;

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
			if (!highScore) return;

			if (displayPrefab == null) {
				displayPrefab = GameObject.Instantiate(highScore);
				displayPrefab.name = "InfoSkull Display Prefab";
				GameObject.Destroy(displayPrefab.GetComponent<TypewriterByWord>());
				GameObject.Destroy(displayPrefab.GetComponent<AudioSource>());
				GameObject.Destroy(displayPrefab.GetComponent<UT_TextScrawl>());
				displayPrefab.AddComponent<DisplayBehaviour>();
				var text1 = displayPrefab.GetComponent<TextMeshProUGUI>();
				text1.color = new Color(1, 1, 1, 0.1f);
				GameObject.Destroy(displayPrefab);
			}

			display = GameObject.Instantiate(displayPrefab, new Vector3(960, 1010, 0), Quaternion.identity,
				GameObject.Find("Game UI").transform);
			display.name = "InfoSkull Display";

			display.GetComponent<TextAnimator_TMP>().enabled = true;
			display.GetComponent<DisplayBehaviour>().enabled = true;
			var text = display.GetComponent<TextMeshProUGUI>();
			text.enabled = true;
		};
	}

	public static class LevelTimer {
		public static ConfigEntry<string> format;
		public static ConfigEntry<bool> onlyBest;
		public static ConfigEntry<bool> saving;
	}

	public static class Display {
		public static ConfigEntry<string> format;
	}

	class DisplayBehaviour : MonoBehaviour {
		bool animatorConfigured;
		bool lastFrameSet;

		void Update() {
			if (lastFrameSet || Display.format.Value == "") {
				lastFrameSet = false;
				return;
			}

			lastFrameSet = true;
			var text = GetComponent<TextMeshProUGUI>();
			if (!text) return;
			text.SetText(Formatter.format(Display.format.Value));
			var animator = (TextAnimator_TMP)GetComponent(typeof(TextAnimator_TMP));
			if (animator && animator.Behaviors.Length == 2 && !animatorConfigured) {
				((ShakeBehavior)animator.Behaviors[0].animation).baseAmplitude = 0.5f;
				((SizeBehavior)animator.Behaviors[1].animation).baseAmplitude = 1.0f;
				animatorConfigured = true;
			}
		}
	}
}