using System.Collections.Generic;
using InfoSkull.builtin.handlers;
using InfoSkull.core;
using InfoSkull.core.components;
using UnityEngine;

namespace InfoSkull.builtin;

public class InfoSkullBuiltins {

	static readonly List<string> BASE_GAME_POS = [
		"High Score",
		"Ascent Header",
		"Tip Header",
		"Header Text",
	];
	public static void init() {
		ElementType.create("text_display")
			.objectCreator(config => {
				var highScore = GameObject.Find("High Score");
				var display = GameObject.Instantiate(highScore, GameObject.Find("Game UI").transform);
				display.name = "TextDisplay";
				return display;
			}).onInstantiate(controller => {
				var format = controller.gameObject.AddComponent<FormatHandler>();
				controller.registerHandler(format);
				var positionHandler = controller.gameObject.AddComponent<PositionHandler>();
				controller.registerHandler(positionHandler);
				var textDisplayHandler = controller.gameObject.AddComponent<TextDisplayHandler>();
				controller.registerHandler(textDisplayHandler);
			}).register();
		
		var baseGameType = ElementType.create("base_game_text")
			.objectCreator(config => {
				return GameObject.Find(config.data["elementName"] as string);
			}).onInstantiate(controller => {
				var baseGamehandler = controller.gameObject.AddComponent<BaseGameHandler>();
				controller.registerHandler(baseGamehandler);
				var handler = controller.gameObject.AddComponent<PositionHandler>();
				controller.registerHandler(handler);
			}).register();
		
		core.InfoSkull.onInitElements += () => {
			foreach (var name in BASE_GAME_POS) {
				var obj = GameObject.Find(name);
				if (obj.GetComponent<ElementController>() != null) continue;
				var baseGameType = ElementType.create("base_game_text")	
					.objectCreator(config => {
						return GameObject.Find(name);
					}).onInstantiate(controller => {
						var baseGamehandler = controller.gameObject.AddComponent<BaseGameHandler>();
						controller.registerHandler(baseGamehandler);
						var moveHandler = controller.gameObject.AddComponent<PositionHandler>();
						controller.registerHandler(moveHandler);
					});
				core.InfoSkull.instantiateType(baseGameType);
			}
		};
	}
}