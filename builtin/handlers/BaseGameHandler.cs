using InfoSkull.core;
using InfoSkull.core.components;
using InfoSkull.core.menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfoSkull.builtin.handlers;

public class BaseGameHandler : ElementHandler {
	ElementController controller;
	
	public override void init(ElementController controller) {
		gameObject.GetComponent<TextMeshProUGUI>().alpha = (bool) controller.config().data["disabled"] ? 0f : 1f;
		controller.liveUnload = () => {
			gameObject.GetComponent<TextMeshProUGUI>().SetText("");
			gameObject.GetComponent<TextMeshProUGUI>().alpha = 1f;
			foreach (var handler in controller.handlers) {
				Destroy(handler);
			}
			Destroy(GetComponent<ContentSizeFitter>());
			core.InfoSkull.elements.Remove(controller);
			Destroy(controller);
		};
		this.controller = controller;
		if (!controller.config().data.ContainsKey("disabled")) controller.config().data["disabled"] = false;
		
		controller.menuSettings().menuButtonCallback += (buttons) => {
			buttons.Add(new MenuButton((bool) controller.config().data["disabled"] ? "Enable" : "Disable", menu => {
				controller.config().data["disabled"] = !(bool) controller.config().data["disabled"];
				menu.hideMenu();
			}));
		};
		
		controller.config().data["elementName"] = gameObject.name;
		
		var contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
		contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
	}
	
	public override void openAdjustUI() {
		gameObject.GetComponent<TextMeshProUGUI>().SetText(gameObject.name);
		gameObject.GetComponent<TextMeshProUGUI>().alpha = 1f;
	}
	
	public override void closeAdjustUI() {
		gameObject.GetComponent<TextMeshProUGUI>().SetText("");
		gameObject.GetComponent<TextMeshProUGUI>().alpha = (bool) controller.config().data["disabled"] ? 0f : 1f;
	}
}