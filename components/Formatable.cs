using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.components;

public class Formatable : MonoBehaviour {
	TMP_InputField inputField;
	public string lastCommittedText = "";
	TextMeshProUGUI text;

	void Awake() {
		text = GetComponent<TextMeshProUGUI>();
		text.raycastTarget = true;

		inputField = gameObject.AddComponent<TMP_InputField>();
		inputField.textComponent = text;

		// I am too stupid to fix the caret and selection not moving with the object, no idea what's going on here
		inputField.selectionColor = new Color(1, 1, 1, 0.0f);
		inputField.caretWidth = 0;
		inputField.contentType = TMP_InputField.ContentType.Standard;
		inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
		inputField.onEndEdit.AddListener(text => {
			if (string.IsNullOrWhiteSpace(text)) inputField.text = "{empty}";
		});

		inputField.onValueChanged.AddListener(text => {
			if (Input.GetKeyDown(KeyCode.Escape)) return;
			lastCommittedText = text;
		});

		var contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
		contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
	}
}