using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace InfoSkull.config.conversions;

public class ConfigVector2 {
	public float x;
	public float y;

	public ConfigVector2(float x, float y) {
		this.x = x;
		this.y = y;
	}
	
	public Vector2 toVector2() {
		return new Vector2(x, y);
	}
	
	public static Vector2 fromDict(object dicto) {
		if (dicto is ConfigVector2 vec) return vec.toVector2();
		var dict = (JObject) dicto;
		return new Vector2((float) dict["x"], (float) dict["y"]);
	}
}