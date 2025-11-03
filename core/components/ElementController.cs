using System;
using System.Collections.Generic;
using InfoSkull.config;
using InfoSkull.config.profiles.elements;
using InfoSkull.core.menu;
using UnityEngine;

namespace InfoSkull.core.components;

public class ElementController : MonoBehaviour {
	ElementType _type;
	ElementConfig _config;
	List<ElementHandler> handlers = new List<ElementHandler>();
	ElementMenuSettings _menuSettings;
	public Func<bool> checkLeaderboardLegal;
	
	internal void init(ElementType type, ElementConfig config = null) {
		_type = type;
		_config = config;
		_menuSettings = new ElementMenuSettings();
	}

	public void registerHandler(ElementHandler handler) {
		handlers.Add(handler);
		handler.init(this);
	}

	public ElementType type() {
		return _type;
	}
	
	public ElementConfig config() {
		return _config;
	}
	
	public ElementMenuSettings menuSettings() {
		return _menuSettings;
	}
	
	public void openAdjustUI() {
		foreach (var handler in handlers) {
			handler.openAdjustUI();
		}
	}

	public void delete() {
		Config.instance.profiles[Config.instance.selectedProfile].elements.Remove(_config);
		InfoSkull.elements.Remove(this);
		Destroy(gameObject);
	}
	
	public void closeAdjustUI() {
		foreach (var handler in handlers) {
			handler.closeAdjustUI();
		}
	}
}