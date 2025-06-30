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
	}
}