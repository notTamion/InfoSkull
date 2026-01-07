using Febucci.UI;
using Febucci.UI.Effects;
using InfoSkull.core;
using InfoSkull.core.components;
using TMPro;
using UnityEngine;

namespace InfoSkull.builtin.handlers;

public class TextDisplayHandler : ElementHandler
{
	ElementController controller;
	TextAnimator_TMP animator;
	bool animatorConfigured;
	bool lastFrameSet;

	TextMeshProUGUI text;

	public override void init(ElementController controller)
	{
		this.controller = controller;

		text = GetComponent<TextMeshProUGUI>();
		text.color = new Color(1, 1, 1, 1.0f);

		animator = GetComponent<TextAnimator_TMP>();
	}

	void Update()
	{
		if (lastFrameSet || core.InfoSkull.isAdjustingUI)
		{
			lastFrameSet = false;
			return;
		}

		lastFrameSet = true;
		if (text) text.SetText(Formatter.format(controller.config().data["format"] as string ?? "{empty}"));

		if (!animatorConfigured && animator && animator.Behaviors.Length == 2)
		{
			((ShakeBehavior)animator.Behaviors[0].animation).baseAmplitude = 0.5f;
			((SizeBehavior)animator.Behaviors[1].animation).baseAmplitude = 1.0f;
			animatorConfigured = true;
		}
	}
}
