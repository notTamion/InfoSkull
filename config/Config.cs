extern alias unityengineold;
using System.Collections.Generic;
using System.IO;
using InfoSkull.config.conversions;
using InfoSkull.config.profiles;
using InfoSkull.config.profiles.elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using unityengineold::UnityEngine;

namespace InfoSkull.config;

public class Config {
	const string CONFIG_PATH = "./BepInEx/config/InfoSkull.json";
	
	public static Config instance;
	public string version = InfoSkullPlugin.VERSION;

	public int selectedProfile;
	public List<ProfileConfig> profiles = new List<ProfileConfig>();

	public static void init() {
		if (instance != null) return;

		if (File.Exists(CONFIG_PATH)) {
			var config = JObject.Parse(File.ReadAllText(CONFIG_PATH));
			upgrade(config);
			instance = JsonConvert.DeserializeObject<Config>(config.ToString());
			return;
		}
		
		instance = new Config();
		instance.defaultConfig();
		instance.save();
	}

	static void upgrade(JObject config) {
	}

	public void save() {
		File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(this));
	}

	void defaultConfig() {
		var profile = ProfileConfig.create("default");
		profile.elements.Add(ElementConfig.create("text_display", new Dictionary<string, object> {
			{"format", "{game_time}"},
			{"position", new ConfigVector2(0.5f, 0.95f)}
		}));
		profiles.Add(profile);
	}
}