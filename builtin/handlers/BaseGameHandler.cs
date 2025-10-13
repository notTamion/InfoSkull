using InfoSkull.core;
using InfoSkull.core.components;
using TMPro;

namespace InfoSkull.builtin.handlers;

public class BaseGameHandler : ElementHandler {
	ElementController controller;
	
	public override void init(ElementController controller) {
		this.controller = controller;
		controller.config().data["elementName"] = gameObject.name;
	}
	
	public override void openAdjustUI() {
		gameObject.GetComponent<TextMeshProUGUI>().SetText(gameObject.name);
	}
	
	public override void closeAdjustUI() {
		gameObject.GetComponent<TextMeshProUGUI>().SetText("");
	}
}