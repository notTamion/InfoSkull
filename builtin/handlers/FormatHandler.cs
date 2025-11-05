using System.Collections.Generic;
using InfoSkull.core;
using InfoSkull.core.components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.builtin.handlers;

public class FormatHandler : ElementHandler {
	ElementController controller;
	TMP_InputField inputField;
	public string lastCommittedText = "";
	TextMeshProUGUI text;
	

	public override bool isLeaderboardLegal() {
			if (!controller.config().data.ContainsKey("format")) return true;
			foreach (var illegal in Formatter.LEADERBOARD_ILLEGAL) {
				if (((string)controller.config().data["format"]).Contains(illegal)) {
					return false;
				}
			}

			return true;
	}

	public override void init(ElementController controller) {
		this.controller = controller;
		text = GetComponent<TextMeshProUGUI>();
		text.raycastTarget = true;

		inputField = gameObject.AddComponent<TMP_InputField>();
		inputField.textComponent = text;

		inputField.contentType = TMP_InputField.ContentType.Standard;
		inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
		inputField.onEndEdit.AddListener(text => {
			if (string.IsNullOrWhiteSpace(text)) inputField.text = "{empty}";
		});

		inputField.onValueChanged.AddListener(text => {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				return;
			}

			lastCommittedText = string.IsNullOrWhiteSpace(text) ? "{empty}" : text;
		});

		// Re-enable to create caret object
		inputField.enabled = true;
		inputField.enabled = false;
		
		var contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
		contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		controller.config().data["format"] = controller.config().data.ContainsKey("format")
			? (string) controller.config().data["format"]
			: "{empty}";
	}
	
	public override void openAdjustUI() {
		inputField.richText = false;
		text.richText = false;
		if (inputField) {
			inputField.text = controller.config().data.ContainsKey("format")
				? (string) controller.config().data["format"]
				: "{empty}";
			inputField.enabled = true;
		}
	}
	
	public override void closeAdjustUI() {
		inputField.richText = true;
		text.richText = true;
		if (inputField) {
			inputField.text = "";
			inputField.enabled = false;
			controller.config().data["format"] = lastCommittedText;
		}
	}
}