extern alias unityengineold;
using Febucci.UI;
using Febucci.UI.Effects;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

namespace InfoSkull.components;

public class Display : MonoBehaviour {
	TextAnimator_TMP animator;
	bool animatorConfigured;
	bool lastFrameSet;
	
	TextMeshProUGUI text;

	void Awake() {
		name = "InfoSkull Display";
		
		gameObject.AddComponent<Formatable>();
		gameObject.AddComponent<Positionable>();

		var rectTransform = GetComponent<RectTransform>();
		rectTransform.anchoredPosition = Plugin.Display.position.Value;

		text = GetComponent<TextMeshProUGUI>();
		text.color = new Color(1, 1, 1, 0.1f);
	
		animator = GetComponent<TextAnimator_TMP>();
	}

	void Update() {
		if (lastFrameSet || Plugin.isAdjustingUI) {
			lastFrameSet = false;
			return;
		}

		lastFrameSet = true;
		if (text) text.SetText(Formatter.format(Plugin.Display.format.Value));

		if (!animatorConfigured && animator && animator.Behaviors.Length == 2) {
			((ShakeBehavior)animator.Behaviors[0].animation).baseAmplitude = 0.5f;
			((SizeBehavior)animator.Behaviors[1].animation).baseAmplitude = 1.0f;
			animatorConfigured = true;
		}
	}
}