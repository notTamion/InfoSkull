using System;
using System.Collections.Generic;

namespace InfoSkull.core.menu;

public class ElementMenuSettings {
	public bool allowDeletion = true;
	public Action<List<MenuButton>> menuButtonCallback;
}