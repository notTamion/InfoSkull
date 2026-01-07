extern alias unityengineold;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfoSkull.config;
using InfoSkull.config.profiles.elements;
using InfoSkull.core.components;
using InfoSkull.patches;
using Mono.CompilerServices.SymbolWriter;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfoSkull.core;

public class InfoSkull
{
	internal static Dictionary<string, ElementType> registeredTypes = new Dictionary<string, ElementType>();
	public static List<ElementController> elements = new List<ElementController>();
	public static Action onInitElements;

	public static bool isAdjustingUI;

	internal static void init()
	{
		Config.init();
		callbacks();
	}

	static void callbacks()
	{
		SceneManager.sceneLoaded += (scene, mode) =>
		{
			var canvas = GameObject.Find("Canvas");
			if (!canvas) return;
			var manager = GameObject.Find("GameManager");
			if (!manager) return;
			canvas.GetComponent<UI_CanvasScaler>().StartCoroutine(initElements());
		};
	}

	public static void selectProfile(int index)
	{
		if (index < 0 || index >= Config.instance.profiles.Count)
		{
			throw new Exception($"Profile index {index} is out of range");
		}
		for (var i = elements.Count - 1; i >= 0; i--)
		{
			elements[i].liveUnload.Invoke();
		}
		Config.instance.selectedProfile = index;
		var canvas = GameObject.Find("Canvas");
		if (!canvas) return;
		var manager = GameObject.Find("GameManager");
		if (!manager) return;
		canvas.GetComponent<UI_CanvasScaler>().StartCoroutine(initElements());
	}

	public static IEnumerator initElements()
	{
		yield return new WaitForEndOfFrame(); // waits until all layout and canvas updates

		elements.Clear();
		RadialMenu.init();

		onInitElements();

		Config.instance.profiles[Config.instance.selectedProfile].elements.ForEach(element =>
		{
			try
			{
				instantiateType(registeredTypes[element.type], element, false);
			}
			catch (Exception) { }
		});
		Config.instance.save();

		checkLeaderboard();
	}

	public static void checkLeaderboard()
	{
		CL_GameManager.gamemode.allowLeaderboardScoring = elements.All(element => element.handlers.All(handler => handler.isLeaderboardLegal()))
								   && !CL_GameManager.HasActiveFlag("leaderboardIllegal")
								   && CL_GameManager.gamemode.allowLeaderboardScoring;

		if (!CL_GameManager.gamemode.allowLeaderboardScoring && !SceneManager.GetSceneByName("Main-Menu").isLoaded)
		{
			GameManagerPatchBuiltin.highScoreQueue = "SESSION IS LEADERBOARD ILLEGAL";
			CL_GameManager.SetGameFlag("leaderboardIllegal", true);
		}
	}

	internal static void openAdjustUI()
	{
		GameObject.Find("Pause Menu").SetActive(false);
		isAdjustingUI = true;
		foreach (var controller in elements)
		{
			try
			{
				controller.openAdjustUI();
			}
			catch (Exception) { }
		}
	}

	internal static void disableAdjustUI()
	{
		foreach (var controller in elements)
		{
			try
			{
				controller.closeAdjustUI();
			}
			catch (Exception) { }
		}
		isAdjustingUI = false;
		Config.instance.save();
		RadialMenu.instance.hideMenu();
		checkLeaderboard();
	}

	public static void registerType(ElementType type)
	{
		if (registeredTypes.ContainsKey(type.name))
		{
			throw new Exception($"Type {type.name} is already registered");
		}
		registeredTypes[type.name] = type;
	}

	public static ElementController instantiateType(ElementType type, ElementConfig config = null, bool save = true)
	{
		if (!registeredTypes.ContainsKey(type.name))
		{
			throw new Exception($"Type {type.name} is not registered");
		}

		if (config == null)
		{
			config = ElementConfig.create(type.name, new Dictionary<string, object>());
			if (save)
			{
				Config.instance.profiles[Config.instance.selectedProfile].elements.Add(config);
			}
		}

		var go = type.creator == null ? new GameObject(type.name) : type.creator.Invoke(config);
		go.transform.parent = GameObject.Find("Game UI").transform;
		var controller = go.AddComponent<ElementController>();
		controller.init(type, config);
		type.onInstantiateAction.Invoke(controller);
		elements.Add(controller);
		if (isAdjustingUI) controller.openAdjustUI();
		return controller;
	}
}
