using System.Collections.Generic;
using InfoSkull.config.profiles.elements;

namespace InfoSkull.config.profiles;

public class ProfileConfig {
	public List<ElementConfig> elements = new List<ElementConfig>();

	public static ProfileConfig create() {
		return new ProfileConfig();
	}
}