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
	
	public static ConfigVector2 fromVector2(Vector2 vec) {
		return new ConfigVector2(vec.x, vec.y);
	}
}