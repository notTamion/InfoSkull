using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace InfoSkull;

[BepInPlugin(GUID, NAME, VERSION)]
public class InfoSkullPlugin : BaseUnityPlugin
{
	const string GUID = "de.tamion.infoskull";
	const string NAME = "InfoSkull";
	public const string VERSION = "2.0.0";

	public static InfoSkullPlugin instance;

	internal static ManualLogSource logger;
	static Harmony harmony;

	void Awake()
	{
		instance = this;
		logger = Logger;

		harmony = new Harmony(GUID);
		harmony.PatchAll();

		core.InfoSkull.init();
		builtin.InfoSkullBuiltins.init();

		logger.LogInfo($"{NAME} has been loaded!");
	}
}
