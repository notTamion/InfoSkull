using System;
using System.Globalization;
using System.Reflection;
using Febucci.UI;
using Febucci.UI.Effects;
using InfoSkull.utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InfoSkull.components {
	public class Display : MonoBehaviour {
		public string elementId;

		TextAnimator_TMP animator;
		bool animatorConfigured;
		bool lastFrameSet;

		TextMeshProUGUI text;
		TMP_InputField inputField;
		RectTransform rectTransform;

		public void Initialize(UiElementConfig config) {
			elementId = config.id;
			rectTransform.anchoredPosition = config.position;
			rectTransform.localEulerAngles = new Vector3(0, 0, config.rotationDegrees % 360);

			if (config.fontSize > 0f) text.fontSize = config.fontSize;
			text.SetText(Formatter.format(config.format ?? string.Empty));

			inputField = GetComponent<TMP_InputField>();
			SyncInputRotation(config.rotationDegrees % 360);
		}

		void Awake() {
			name = "InfoSkull Display";

			rectTransform = GetComponent<RectTransform>();
			if (!rectTransform) rectTransform = gameObject.AddComponent<RectTransform>();

			text = GetComponent<TextMeshProUGUI>();
			if (!text) text = gameObject.AddComponent<TextMeshProUGUI>();
			text.richText = true;
			text.color = new Color(1, 1, 1, 0.1f); // Default alpha of 0.1

			if (!GetComponent<Formatable>()) gameObject.AddComponent<Formatable>();
			if (!GetComponent<Positionable>()) gameObject.AddComponent<Positionable>();

			animator = GetComponent<TextAnimator_TMP>();
			inputField = GetComponent<TMP_InputField>();
		}

		public void Update() {
			if (Plugin.isAdjustingUI) {
		
				if (inputField && !inputField.enabled) inputField.enabled = true;
				if (text.text != inputField.text) {
					text.text = inputField.text;
				}
				{
					string rawEditing = inputField ? inputField.text : (text ? text.text : string.Empty);
					string rawLower = rawEditing != null ? rawEditing.ToLowerInvariant() : string.Empty;
					float alphaLive = 0.1f;
					if (!string.IsNullOrEmpty(rawLower)) {
						int startIndex = rawLower.IndexOf("{alpha:");
						if (startIndex >= 0) {
							int colonIndex = rawLower.IndexOf(':', startIndex);
							int endIndex = rawLower.IndexOf('}', colonIndex);
							if (colonIndex != -1 && endIndex != -1) {
								string alphaStr = rawLower.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim();
								if (float.TryParse(alphaStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)) {
									alphaLive = UnityEngine.Mathf.Clamp(parsed, 0.1f, 1f);
								}
							}
						}
					}
					var c = text.color; c.a = alphaLive; text.color = c;
				}
				bool isActive = (inputField && inputField.isFocused) || (EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject);
				if (isActive) {
					bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
					if (ctrl && Input.GetKeyDown(KeyCode.R)) {
						var cfg = FindConfig();
						if (cfg != null) {
							cfg.rotationDegrees = (cfg.rotationDegrees + 90) % 360;
							rectTransform.localEulerAngles = new Vector3(0, 0, cfg.rotationDegrees);
							SyncInputRotation((int)cfg.rotationDegrees);
							ConfigService.Save();
						}
					}

					if (Input.GetKeyDown(KeyCode.Delete)) {
						var cfg = FindConfig();
						if (cfg != null) {
							ConfigService.Data.elements.Remove(cfg);
							ConfigService.Save();
							Plugin.displayTextElements.Remove(gameObject);
							UnityEngine.Object.Destroy(gameObject);
							return;
						}
					}
				}
			} else {
				if (inputField && inputField.enabled) inputField.enabled = false;
				var config = FindConfig();
				if (config != null) {
					string raw = config.format ?? string.Empty;
					
					if (string.IsNullOrWhiteSpace(raw) || !raw.Contains("{")) {
						text.enabled = false;
						return;
					}
					
					float alphaValue = 0.1f; // default alpha
					var rawLower = raw.ToLowerInvariant();
					if (rawLower.Contains("{alpha:")) {
						int startIndex = rawLower.IndexOf("{alpha:");
						int colonIndex = rawLower.IndexOf(':', startIndex);
						int endIndex = rawLower.IndexOf('}', colonIndex);
						if (colonIndex != -1 && endIndex != -1) {
							string alphaStr = rawLower.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim();
							if (float.TryParse(alphaStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedAlpha)) {
								alphaValue = UnityEngine.Mathf.Clamp(parsedAlpha, 0.1f, 1f);
							}
						}
					}
					
					string formatWithoutAlpha = raw;
					if (rawLower.Contains("{alpha:")) {
						int startIndex = rawLower.IndexOf("{alpha:");
						int endIndex = rawLower.IndexOf('}', startIndex);
						if (endIndex != -1) {
							formatWithoutAlpha = raw.Remove(startIndex, endIndex - startIndex + 1);
						}
					}
					
					string formatted = Formatter.format(formatWithoutAlpha);
					
					// Set text alpha AFTER formatting
					Color textColor = text.color;
					textColor.a = alphaValue;
					text.color = textColor;
					
					if (string.IsNullOrWhiteSpace(formatted)) {
						text.enabled = false;
					} else {
						text.enabled = true;
						text.text = formatted;
					}
				} else {
					text.enabled = false;
				}
			}
		}

		public void ApplyRotationToInput() {
			var cfg = FindConfig();
			if (cfg != null) SyncInputRotation(cfg.rotationDegrees % 360);
		}

		// pretty cool rotating thing so everything gets synced up.
		void SyncInputRotation(int degrees) {
			if (!inputField) return;
			try {
				// Rotate main input hierarchy
				var inputRect = inputField.GetComponent<RectTransform>();
				if (inputRect) inputRect.localEulerAngles = new Vector3(0, 0, degrees);
				// Rotate TMP text component
				if (text && text.rectTransform) text.rectTransform.localEulerAngles = new Vector3(0, 0, degrees);
				// Rotate viewport and text area containers
				if (inputField.textViewport) inputField.textViewport.localEulerAngles = new Vector3(0, 0, degrees);
				var textArea = inputField.transform.Find("Text Area");
				if (textArea) textArea.localEulerAngles = new Vector3(0, 0, degrees);
				// Rotate text components
				var tc = inputField.textComponent as TMP_Text;
				if (tc && tc.rectTransform) tc.rectTransform.localEulerAngles = new Vector3(0, 0, degrees);
				var pc = inputField.placeholder as TMP_Text;
				if (pc && pc.rectTransform) pc.rectTransform.localEulerAngles = new Vector3(0, 0, degrees);
				// Rotate caret rect (private field)
				var caretField = typeof(TMP_InputField).GetField("m_CaretRectTrans", BindingFlags.NonPublic | BindingFlags.Instance) 
							  ?? typeof(TMP_InputField).GetField("caretRectTrans", BindingFlags.NonPublic | BindingFlags.Instance);
				var caret = caretField?.GetValue(inputField) as RectTransform;
				if (caret) caret.localEulerAngles = new Vector3(0, 0, degrees);
				// Rotate selection highlight (search recursively)
				var sel = inputField.transform.Find("Selection") ?? inputField.transform.GetComponentInChildren<RectTransform>(true);
				// Try to find any child named Selection via traversal
				if (!sel) {
					foreach (RectTransform r in inputField.GetComponentsInChildren<RectTransform>(true)) {
						if (r.name == "Selection") { sel = r; break; }
					}
				}
				if (sel) sel.localEulerAngles = new Vector3(0, 0, degrees);
			} catch { }
		}

		UiElementConfig FindConfig() {
			foreach (var e in ConfigService.Data.elements) if (e.id == elementId) return e;
			return null;
		}
	}
}

