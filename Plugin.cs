using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InfoSkull.components;
using InfoSkull.patches;
using InfoSkull.utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfoSkull;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
	const string GUID = "InfoSkull";
	const string NAME = "InfoSkull";
	const string VERSION = "1.0.2";

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
		Formatter.EXTRA_JUMPS,
		Formatter.ROACHES,
		Formatter.ROACHES_BANKED_THIS_RUN
	];

	static readonly List<string> UI_POS = [
		"High Score",
		"Ascent Header",
		"Tip Header",
		"Header Text"
	];

	static readonly string[] TOKENS = new[] {
		Formatter.LEVEL,
		Formatter.LEVEL_TIME,
		Formatter.HEIGHT,
		Formatter.BEST_LEVEL_TIME,
		Formatter.ASCENT_RATE,
		Formatter.GAME_TIME,
		Formatter.CLOCK,
		Formatter.LEFT_STAMINA,
		Formatter.RIGHT_STAMINA,
		Formatter.MASS_HEIGHT,
		Formatter.MASS_SPEED,
		Formatter.MASS_ACC_MULT,
		Formatter.MASS_DISTANCE,
		Formatter.SCORE,
		Formatter.HIGH_SCORE,
		Formatter.ASCENT,
		Formatter.VELOCITY,
		Formatter.HEALTH,
		Formatter.EXTRA_JUMPS,
		Formatter.ROACHES,
		Formatter.ROACHES_BANKED_THIS_RUN,
		Formatter.EMPTY
	};
	static readonly Regex TokenRegex = new Regex("\\{[^}]+\\}", RegexOptions.Compiled);

	public static bool isAdjustingUI;
	public static GameObject adjustUI;
	public static GameObject levelTimer;
	public static readonly List<GameObject> displayTextElements = new List<GameObject>();
	public static GameObject addElementButton;
	public static GameObject saveButton;
	public static GameObject adjustOverlay;
	public static int roachesBankedThisRun;

	public static void checkLeaderboardLegality() {
		CL_GameManager.gamemode.allowLeaderboardScoring = !LEADERBOARD_ILLEGAL.Any(illegal =>
				ConfigService.Data.levelTimerFormat.Contains(illegal) ||
				(ConfigService.Data.elements != null && ConfigService.Data.elements.Any(e => !string.IsNullOrEmpty(e.format) && e.format.Contains(illegal))))
				&& CL_GameManager.gamemode.allowLeaderboardScoring
				                                        && !CL_GameManager.HasActiveFlag("leaderboardIllegal");

		if (!CL_GameManager.gamemode.allowLeaderboardScoring && !SceneManager.GetSceneByName("Main-Menu").isLoaded) {
			GameManagerPatch.highScoreQueue = "RUN IS LEADERBOARD ILLEGO";
			Commands.sendMessage("Leaderboard illegal variable used run will not be scored");
			CL_GameManager.SetGameFlag("leaderboardIllegal", true);
		}
	}

	void Awake() {
		instance = this;
		ConfigService.LoadOrInitialize();
		
		// On first run, the default config has one element. We replace it with the two desired starter elements.
		// This check is specific enough not to trigger on user-modified configs.
		if (ConfigService.Data.elements.Count == 1 && ConfigService.Data.elements[0].format.Contains(Formatter.ROACHES_BANKED_THIS_RUN))
		{
			ConfigService.Data.elements.Clear();
			
			// Add Clock element
			ConfigService.Data.elements.Add(new UiElementConfig {
				id = Guid.NewGuid().ToString("N"),
				format = Formatter.CLOCK,
				position = new Vector2(0, Screen.height / 2f - 50),
				rotationDegrees = 0,
				fontSize = 0f
			});
			
			// Add Level Timer element
			ConfigService.Data.elements.Add(new UiElementConfig {
				id = Guid.NewGuid().ToString("N"),
				format = Formatter.LEVEL_TIME,
				position = new Vector2(0, Screen.height / 2f - 100),
				rotationDegrees = 0,
				fontSize = 0f
			});
			ConfigService.Save();
		}
		
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
				levelTimer = GameObject.Instantiate(highScore, GameObject.Find("Game UI").transform);
				levelTimer.AddComponent<components.LevelTimer>();
				
				foreach (GameObject go in displayTextElements) if (go) UnityEngine.Object.Destroy(go);
				displayTextElements.Clear();
				foreach (var elementCfg in ConfigService.Data.elements) {
					var go = new GameObject($"IS Element {elementCfg.id.Substring(0, 6)}", typeof(RectTransform));
					go.transform.SetParent(GameObject.Find("Game UI").transform, false);
					var tmp = go.AddComponent<TextMeshProUGUI>();
					var templateText = GameObject.Find("High Score")?.GetComponent<TextMeshProUGUI>();
					if (templateText) {
						tmp.font = templateText.font;
						tmp.fontMaterial = templateText.fontMaterial;
						tmp.fontSize = templateText.fontSize;
						tmp.alignment = templateText.alignment;
					}
					if (elementCfg.fontSize > 0f) tmp.fontSize = elementCfg.fontSize;
					var dyn = go.AddComponent<InfoSkull.components.Display>();
					dyn.Initialize(elementCfg);
					displayTextElements.Add(go);
				}

				// Restore and apply stored UI positions for built-in elements
				foreach (var ui in UI_POS) {
					var gameObject = GameObject.Find(ui);
					if (!gameObject) continue;
					var rectTransform = gameObject.GetComponent<RectTransform>();
					if (ConfigService.Data.uiPositions.TryGetValue(ui, out var saved)) {
						rectTransform.anchoredPosition = saved;
					}
					else {
						ConfigService.Data.uiPositions[ui] = rectTransform.anchoredPosition;
						ConfigService.Save();
					}
					gameObject.AddComponent<Positionable>();
				}
			}
		};
	}

	public static void enableUIAdjust() {
		var template = adjustUI ? adjustUI : GameObject.Find("Report Bug");
		GameObject.Find("Pause Menu").SetActive(false);
		isAdjustingUI = true;
		var gameUI = GameObject.Find("Game UI");
		if (!adjustOverlay && gameUI) {
			adjustOverlay = new GameObject("Adjust UI Overlay", typeof(RectTransform));
			adjustOverlay.transform.SetParent(gameUI.transform, false);
			var ort = adjustOverlay.GetComponent<RectTransform>();
			ort.anchorMin = ort.anchorMax = new Vector2(0.5f, 0.5f);
			ort.sizeDelta = Vector2.zero;
			ort.anchoredPosition = Vector2.zero;
		}
		if (adjustOverlay) adjustOverlay.SetActive(true);

		// Show raw formats for editing and ensure rotation visuals synced
		if (levelTimer) {
			var li = levelTimer.GetComponent<TMP_InputField>();
			var lt = levelTimer.GetComponent<TextMeshProUGUI>();
			var lf = levelTimer.GetComponent<Formatable>();
			if (lt) lt.SetText(ConfigService.Data.levelTimerFormat);
			if (li) { li.text = ConfigService.Data.levelTimerFormat; li.enabled = true; li.caretPosition = li.text.Length; }
			if (lf) lf.lastCommittedText = ConfigService.Data.levelTimerFormat;
		}
		foreach (var go in displayTextElements) {
			if (!go) continue;
			var idc = go.GetComponent<InfoSkull.components.Display>();
			if (!idc) continue;
			var cfg = ConfigService.Data.elements.Find(e => e.id == idc.elementId);
			if (cfg == null) continue;
			var input = go.GetComponent<TMP_InputField>();
			var text = go.GetComponent<TextMeshProUGUI>();
			var form = go.GetComponent<Formatable>();
			if (text) text.SetText(cfg.format);
			if (input) { input.text = cfg.format; input.enabled = true; input.caretPosition = input.text.Length; }
			if (form) form.lastCommittedText = cfg.format;
		}
		foreach (var go in displayTextElements) {
			var dyn = go ? go.GetComponent<InfoSkull.components.Display>() : null;
			if (dyn) dyn.ApplyRotationToInput();
		}
		
		foreach (var ui in UI_POS) {
			var gameObject = GameObject.Find(ui);
			if (!gameObject) continue;
			gameObject.GetComponent<TextMeshProUGUI>().SetText(ui);
		}
		
		if (!addElementButton && adjustOverlay && template) {
			addElementButton = GameObject.Instantiate(template, adjustOverlay.transform);
			addElementButton.name = "Add Element";
			var comp1 = addElementButton.GetComponent("UI_AnimateOnSelect");
			if (comp1) UnityEngine.Object.Destroy(comp1);
			var comp2 = addElementButton.GetComponent("UI_MenuButton");
			if (comp2) UnityEngine.Object.Destroy(comp2);
			var button = addElementButton.GetComponent<UnityEngine.UI.Button>();
			var labelText = addElementButton.GetComponentInChildren<TextMeshProUGUI>();
			if (labelText) labelText.SetText("Add Element");
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => { addDisplayTextElement(); });
			button.interactable = true;
			addElementButton.transform.localPosition = new Vector3(703.213f, 613.6124f, 0f);
			addElementButton.transform.SetAsLastSibling();
		}
		// Save button just below Add Element
		if (!saveButton && adjustOverlay && template) {
			saveButton = GameObject.Instantiate(template, adjustOverlay.transform);
			saveButton.name = "Save and Exit";
			var comp1s = saveButton.GetComponent("UI_AnimateOnSelect");
			if (comp1s) UnityEngine.Object.Destroy(comp1s);
			var comp2s = saveButton.GetComponent("UI_MenuButton");
			if (comp2s) UnityEngine.Object.Destroy(comp2s);
			var buttonS = saveButton.GetComponent<UnityEngine.UI.Button>();
			var labelTextS = saveButton.GetComponentInChildren<TextMeshProUGUI>();
			if (labelTextS) labelTextS.SetText("Save and Exit");
			buttonS.onClick.RemoveAllListeners();
			buttonS.onClick.AddListener(() => { saveAdjustState(); disableUIAdjust(); ShowPauseMenu(); });
			buttonS.interactable = true;
			saveButton.transform.localPosition = new Vector3(703.213f, 613.6124f - 60f, 0f);
			saveButton.transform.SetAsLastSibling();
		}
	}

	public static void disableUIAdjust() {
		// Persist any current edits, then clear temporary UI
		saveAdjustState();
		// Clear display text element visible text safely and prune destroyed entries
		for (int i = displayTextElements.Count - 1; i >= 0; i--) {
			var go = displayTextElements[i];
			if (!go) { displayTextElements.RemoveAt(i); continue; }
			var t = go.GetComponent<TextMeshProUGUI>();
			if (t) t.SetText("");
			var inp = go.GetComponent<TMP_InputField>();
			if (inp) inp.enabled = false;
		}
		foreach (var ui in UI_POS) {
			var gameObject = GameObject.Find(ui);
			if (!gameObject) continue;
			var ut = gameObject.GetComponent<TextMeshProUGUI>();
			if (ut) ut.SetText("");
		}
		
		if (levelTimer) {
			var li = levelTimer.GetComponent<TMP_InputField>();
			var lt = levelTimer.GetComponent<TextMeshProUGUI>();
			if (li) li.enabled = false;
			if (lt) lt.SetText("");
		}

		isAdjustingUI = false;
		if (addElementButton) { UnityEngine.Object.Destroy(addElementButton); addElementButton = null; }
		if (saveButton) { UnityEngine.Object.Destroy(saveButton); saveButton = null; }
		if (adjustOverlay) adjustOverlay.SetActive(false);
		checkLeaderboardLegality();
	}
	
	static void ShowPauseMenu() {
		// Prefer invoking the game's pause flow so UI is rebuilt consistently
		if (CL_GameManager.gMan != null) {
			CL_GameManager.gMan.Pause();
			return;
		}
		// Fallback: toggle object if manager not available yet
		var pauseMenu = GameObject.Find("Pause Menu");
		if (pauseMenu) pauseMenu.SetActive(true);
	}

	static void addDisplayTextElement() {
		var cfg = new UiElementConfig {
			id = Guid.NewGuid().ToString("N"),
			format = string.Empty,
			position = Vector2.zero,
			rotationDegrees = 0
		};
		ConfigService.Data.elements.Add(cfg);
		ConfigService.Save();
		var gameUI = GameObject.Find("Game UI");
		if (!gameUI) return;
		var go = new GameObject($"IS Element {cfg.id.Substring(0, 6)}", typeof(RectTransform));
		go.transform.SetParent(gameUI.transform, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = cfg.position;
		var tmp = go.AddComponent<TextMeshProUGUI>();
		tmp.richText = true;
		tmp.text = string.Empty;
		tmp.color = new Color(1, 1, 1, 0.1f);
		var templateText = GameObject.Find("High Score")?.GetComponent<TextMeshProUGUI>();
		if (templateText) {
			tmp.font = templateText.font;
			tmp.fontMaterial = templateText.fontMaterial;
			tmp.fontSize = templateText.fontSize;
			tmp.alignment = templateText.alignment;
		}
		var dyn = go.AddComponent<InfoSkull.components.Display>();
		dyn.Initialize(cfg);
		displayTextElements.Add(go);
		var input = go.GetComponent<TMP_InputField>();
		if (input) { input.text = cfg.format; input.enabled = true; input.ActivateInputField(); }
		ConfigService.Save();
	}

	static string SanitizeFormat(string raw) {
		// Keep raw intact, but strip any tokens that are not known variables so rich text survives
		if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
		var matches = TokenRegex.Matches(raw);
		var allowed = new HashSet<string>(TOKENS);
		foreach (Match m in matches) {
			var tok = m.Value;
			// Preserve alpha tokens like {alpha:0.5}
			if (tok.StartsWith("{alpha:", StringComparison.OrdinalIgnoreCase)) continue;
			if (!allowed.Contains(tok)) raw = raw.Replace(tok, "");
		}
		return raw.Trim();
	}

	static void saveAdjustState() {
		// Persist display text elements
		foreach (var go in displayTextElements) {
			if (!go) continue;
			var id = go.GetComponent<InfoSkull.components.Display>().elementId;
			var cfg = ConfigService.Data.elements.Find(e => e.id == id);
			var rect = go.GetComponent<RectTransform>();
			var form = go.GetComponent<Formatable>();
			if (cfg != null) {
				cfg.format = SanitizeFormat(form.lastCommittedText);
				cfg.position = rect.anchoredPosition;
				var txt = go.GetComponent<TextMeshProUGUI>();
				if (txt) cfg.fontSize = txt.fontSize;
			}
		}

		// Persist built-in formats/positions
		if (levelTimer) {
			var lf = levelTimer.GetComponent<Formatable>();
			var lr = levelTimer.GetComponent<RectTransform>();
			ConfigService.Data.levelTimerFormat = SanitizeFormat(lf.lastCommittedText);
			ConfigService.Data.levelTimerPosition = lr.anchoredPosition;
		}
		// Save built-in UI positions
		foreach (var ui in UI_POS) {
			var gameObject = GameObject.Find(ui);
			if (!gameObject) continue;
			ConfigService.Data.uiPositions[ui] = gameObject.GetComponent<RectTransform>().anchoredPosition;
		}
		ConfigService.Save();
	}

	static void RebuildUI() {
		for (int i = displayTextElements.Count - 1; i >= 0; i--) {
			var go = displayTextElements[i];
			if (go) UnityEngine.Object.Destroy(go);
		}
		displayTextElements.Clear();
		var gameUI = GameObject.Find("Game UI");
		if (!gameUI) return;
		// Recreate display text elements
		foreach (var elementCfg in ConfigService.Data.elements) {
			var go = new GameObject($"IS Element {elementCfg.id.Substring(0, 6)}", typeof(RectTransform));
			go.transform.SetParent(gameUI.transform, false);
			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = elementCfg.position;
			var tmp = go.AddComponent<TextMeshProUGUI>();
			tmp.richText = true;
			tmp.text = string.Empty;
			tmp.color = new Color(1, 1, 1, 0.1f);
			var templateText = GameObject.Find("High Score")?.GetComponent<TextMeshProUGUI>();
			if (templateText) {
				tmp.font = templateText.font;
				tmp.fontMaterial = templateText.fontMaterial;
				tmp.fontSize = templateText.fontSize;
				tmp.alignment = templateText.alignment;
			}
			if (elementCfg.fontSize > 0f) tmp.fontSize = elementCfg.fontSize;
			var dyn = go.AddComponent<InfoSkull.components.Display>();
			dyn.Initialize(elementCfg);
			displayTextElements.Add(go);
		}
		// Reapply built-in positions and formats
		if (levelTimer) {
			var lr = levelTimer.GetComponent<RectTransform>();
			if (lr) lr.anchoredPosition = ConfigService.Data.levelTimerPosition;
			var lt = levelTimer.GetComponent<TextMeshProUGUI>();
			if (lt) lt.SetText(ConfigService.Data.levelTimerFormat);
		}
	}
}