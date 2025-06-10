using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Febucci.UI;
using Febucci.UI.Effects;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfoSkull;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
	const string GUID = "InfoSkull";
	const string NAME = "InfoSkull";
	const string VERSION = "1.0.0";

	public static Plugin instance;

	internal static ManualLogSource logger;
	static Harmony harmony;

	public static class LevelTimer {
		public static ConfigEntry<string> format;
		public static ConfigEntry<bool> onlyBest;
		public static ConfigEntry<bool> saving;
	}

	public static class Display {
		public static ConfigEntry<string> format;
	}

	void Awake() {
		instance = this;

		LevelTimer.format = Config.Bind("LevelTimer", "format", $"{Formatter.LEVEL_TIME} / {Formatter.BEST_LEVEL_TIME}", "Format to display");
		LevelTimer.onlyBest = Config.Bind("LevelTimer", "only-best", false, "Whether to only show new bests");
		LevelTimer.saving = Config.Bind("LevelTimer", "saving", true, "Whether to save best times in the games stats file");
		
		Display.format = Config.Bind("Display", "format", $"{Formatter.CLOCK}", "Format to display");
		
		logger = Logger;
		harmony = new Harmony(GUID);

		harmony.PatchAll();
		
		callbacks();

		logger.LogInfo($"{NAME} is loaded!");
	}
	
	GameObject displayPrefab;
	GameObject display;
	void callbacks() {
		SceneManager.sceneLoaded += (scene, mode) => {
			GameObject highScore = GameObject.Find("High Score");
			if (!highScore) {
				return;
			}
			if (displayPrefab == null) {
				displayPrefab = GameObject.Instantiate(highScore);
				displayPrefab.name = "InfoSkull Display Prefab";
				GameObject.Destroy(displayPrefab.GetComponent<TypewriterByWord>());
				GameObject.Destroy(displayPrefab.GetComponent<AudioSource>());
				GameObject.Destroy(displayPrefab.GetComponent<UT_TextScrawl>());
				displayPrefab.AddComponent<DisplayBehaviour>();
				TextMeshProUGUI text1 = displayPrefab.GetComponent<TextMeshProUGUI>();
				text1.color = new Color(1, 1, 1, 0.1f);
				GameObject.Destroy(displayPrefab);
			}
			
			display = GameObject.Instantiate(displayPrefab, new Vector3(960, 1010, 0), Quaternion.identity, GameObject.Find("Game UI").transform);
			display.name = "InfoSkull Display";
			
			display.GetComponent<TextAnimator_TMP>().enabled = true;
			display.GetComponent<DisplayBehaviour>().enabled = true;
			TextMeshProUGUI text = display.GetComponent<TextMeshProUGUI>();
			text.enabled = true;
		};
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
			TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
			text.SetText(Formatter.format(Display.format.Value));
			TextAnimator_TMP animator = (TextAnimator_TMP) GetComponent(typeof(TextAnimator_TMP));
			if (animator.Behaviors.Length == 2 && !animatorConfigured) {
				((ShakeBehavior) animator.Behaviors[0].animation).baseAmplitude = 0.5f;
				((SizeBehavior) animator.Behaviors[1].animation).baseAmplitude = 1.0f;
				animatorConfigured = true;
			}
		}
	}
}