using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfoSkull.config.profiles.elements;

public class ElementConfig {
	[JsonProperty]
	internal string type;
	public Dictionary<string, object> data;
	
	internal static ElementConfig create(string type, Dictionary<string, object> data) {
		return new ElementConfig() {
			type = type,
			data = data
		};
	}
}