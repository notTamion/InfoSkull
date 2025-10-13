using System.Collections.Generic;
using InfoSkull.config.profiles.elements;
using UnityEngine;

namespace InfoSkull.core.components;

public class ElementController : MonoBehaviour {
	ElementType _type;
	ElementConfig _config;
	List<ElementHandler> handlers = new List<ElementHandler>();
	
	internal void init(ElementType type, ElementConfig config = null) {
		_type = type;
		_config = config;
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
	
	public void openAdjustUI() {
		foreach (var handler in handlers) {
			handler.openAdjustUI();
		}
	}
	
	public void closeAdjustUI() {
		foreach (var handler in handlers) {
			handler.closeAdjustUI();
		}
	}
}