using System;
using InfoSkull.core.components;

namespace InfoSkull.core.menu;

public class MenuButton
{
	public string label;
	public Action<RadialMenu> onClick;

	public MenuButton(string label, Action<RadialMenu> onClick)
	{
		this.label = label;
		this.onClick = onClick;
	}

	public static MenuButton create(string label, Action<RadialMenu> onClick)
	{
		return new MenuButton(label, onClick);
	}
}
