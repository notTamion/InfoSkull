extern alias unityengineold;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using InfoSkull.builtin;
using InfoSkull.config.conversions;
using InfoSkull.config.profiles;
using InfoSkull.config.profiles.elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InfoSkull.config;

public class Config
{
	public static string CONFIG_PATH = Paths.ConfigPath + "/InfoSkull.json";

	public static Config instance;
	public string version = InfoSkullPlugin.VERSION;

	public int selectedProfile;
	public List<ProfileConfig> profiles = new List<ProfileConfig>();

	public static ProfileConfig currentProfile()
	{
		return instance.profiles[instance.selectedProfile];
	}

	public static void init()
	{
		if (instance != null) return;

		if (File.Exists(CONFIG_PATH))
		{
			var config = JObject.Parse(File.ReadAllText(CONFIG_PATH));
			upgrade(config);
			instance = JsonConvert.DeserializeObject<Config>(config.ToString());
			return;
		}

		instance = new Config();
		instance.defaultConfig();
		instance.save();
	}

	static void upgrade(JObject config)
	{
	}

	public void save()
	{
		File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(this));
	}

	void defaultConfig()
	{
		var profile = ProfileConfig.create();
		defaultProfile(profile);
		profiles.Add(profile);
	}

	public static void defaultProfile(ProfileConfig profile)
	{
		profile.elements.Add(ElementConfig.create("text_display", new Dictionary<string, object> {
			{"format", "{game_time}"},
			{"position", new ConfigVector2(0.5f, 0.98f)}
		}));

		foreach (var name in InfoSkullBuiltins.BASE_GAME_POS)
		{
			profile.elements.Add(ElementConfig.create("base_game_text", new Dictionary<string, object> {
				{"elementName", name}
			}));
		}
	}
}
