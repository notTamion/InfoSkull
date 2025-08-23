using UnityEngine;
using InfoSkull.utils;

namespace InfoSkull.components;

public class LevelTimer : MonoBehaviour {
	void Awake() {
		name = "InfoSkull LevelTimer";
		
		gameObject.AddComponent<Formatable>();
		gameObject.AddComponent<Positionable>();

		var rectTransform = GetComponent<RectTransform>();
		rectTransform.anchoredPosition = ConfigService.Data.levelTimerPosition;
	}
}