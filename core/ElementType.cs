using System;
using System.Collections.Generic;
using InfoSkull.config.profiles.elements;
using InfoSkull.core.components;
using UnityEngine;

namespace InfoSkull.core;

public class ElementType {
	internal string name;
	internal Action<ElementController> onInstantiateAction;
	internal Func<ElementConfig, GameObject> creator;

	public static ElementType create(string name) {
		return new ElementType {
			name = name
		};
	}

	public ElementType onInstantiate(Action<ElementController> onInstantiate) {
		onInstantiateAction += onInstantiate;
		return this;
	}

	public ElementType objectCreator(Func<ElementConfig, GameObject> creator) {
		this.creator = creator;
		return this;
	}

	public ElementType register() {
		InfoSkull.registerType(this);
		return this;
	}
}