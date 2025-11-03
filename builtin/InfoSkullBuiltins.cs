using System.Collections.Generic;
using InfoSkull.builtin.handlers;
using InfoSkull.core;
using UnityEngine;

namespace InfoSkull.builtin;

public class InfoSkullBuiltins {

	internal static readonly List<string> BASE_GAME_POS = [
		"High Score",
		"Ascent Header",
		"Tip Header",
		"Header Text",
	];
	
	static GameObject textDisplayPrefab;
	
	public static void init() {
		core.InfoSkull.onInitElements += () => {
			if (!textDisplayPrefab) {
				var highScore = GameObject.Find("High Score");
				textDisplayPrefab = GameObject.Instantiate(highScore);
				textDisplayPrefab.name = "TextDisplay Prefab";
				GameObject.DontDestroyOnLoad(textDisplayPrefab);
			}
		};
		
		ElementType.create("text_display")
			.objectCreator(config => {
				var display = GameObject.Instantiate(textDisplayPrefab, GameObject.Find("Game UI").transform);
				display.name = "TextDisplay";
				return display;
			}).onInstantiate(controller => {
				controller.gameObject.GetComponent<BaseGameHandler>();
				var format = controller.gameObject.AddComponent<FormatHandler>();
				controller.registerHandler(format);
				var positionHandler = controller.gameObject.AddComponent<PositionHandler>();
				controller.registerHandler(positionHandler);
				var textDisplayHandler = controller.gameObject.AddComponent<TextDisplayHandler>();
				controller.registerHandler(textDisplayHandler);
			}).register();
		
		ElementType.create("base_game_text")
			.objectCreator(config => {
				return GameObject.Find(config.data["elementName"] as string);
			}).onInstantiate(controller => {
				controller.menuSettings().allowDeletion = false;
				var baseGamehandler = controller.gameObject.AddComponent<BaseGameHandler>();
				controller.registerHandler(baseGamehandler);
				var handler = controller.gameObject.AddComponent<PositionHandler>();
				controller.registerHandler(handler);
			}).allowCreationByMenu(false)
			.register();
	}
}