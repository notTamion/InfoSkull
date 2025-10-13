extern alias unityengineold;
using System;
using System.Collections;
using System.Collections.Generic;
using InfoSkull.config;
using InfoSkull.config.profiles.elements;
using InfoSkull.core.components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfoSkull.core;

public class InfoSkull {
	static Dictionary<string, ElementType> registeredTypes = new Dictionary<string, ElementType>();
	internal static List<ElementController> elements = new List<ElementController>();
	public static Action onInitElements;
	
	public static bool isAdjustingUI;

	internal static void init() {
		Config.init();
		callbacks();
	}
	
	static void callbacks() {
		SceneManager.sceneLoaded += (scene, mode) => {
			GameObject.Find("High Score").GetComponent<UT_TextScrawl>().StartCoroutine(initElements());
		};
	}
	
	static IEnumerator initElements()
	{
		yield return new WaitForEndOfFrame(); // waits until all layout and canvas updates
		
		elements.Clear();

		Config.instance.profiles[Config.instance.selectedProfile].elements.ForEach(element => {
			instantiateType(registeredTypes[element.type], element, false);
		});
		
		onInitElements();
		Config.instance.save();
	}

	internal static void openAdjustUI() {
		GameObject.Find("Pause Menu").SetActive(false);
		isAdjustingUI = true;
		foreach (var controller in elements) {
			controller.openAdjustUI();
		}
	}
	
	internal static void disableAdjustUI() {
		foreach (var controller in elements) {
			controller.closeAdjustUI();
		}
		isAdjustingUI = false;
		Config.instance.save();
	}
	
	public static void registerType(ElementType type) {
		if(registeredTypes.ContainsKey(type.name)) {
			throw new Exception($"Type {type.name} is already registered");
		}
		registeredTypes[type.name] = type;
	}

	public static void instantiateType(ElementType type, ElementConfig config = null, bool save = true) {
		if(!registeredTypes.ContainsKey(type.name)) {
			throw new Exception($"Type {type.name} is not registered");
		}

		if (config == null) {
			config = ElementConfig.create(type.name, new Dictionary<string, object>());
			if (save) {
				Config.instance.profiles[Config.instance.selectedProfile].elements.Add(config);
			}
		}
		
		var go = type.creator == null ? new GameObject(type.name) : type.creator.Invoke(config);
		go.transform.parent = GameObject.Find("Game UI").transform;
		var controller = go.AddComponent<ElementController>();
		controller.init(type, config);
		type.onInstantiateAction.Invoke(controller);
		elements.Add(controller);
	}
}