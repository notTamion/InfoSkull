using System.Collections.Generic;
using InfoSkull.config.profiles.elements;

namespace InfoSkull.config.profiles;

public class ProfileConfig {
	public string name;
	public List<ElementConfig> elements = new List<ElementConfig>();

	public static ProfileConfig create(string name) {
		return new ProfileConfig{
			name = name
		};
	}
}