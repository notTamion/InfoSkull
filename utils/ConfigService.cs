using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using BepInEx;
using System.Linq;

namespace InfoSkull.utils {
	public class UiElementConfig {
		public string id;
		public string format;
		public Vector2 position;
		public int rotationDegrees;
		public float fontSize;
	}

	public class PresetData {
		public List<UiElementConfig> elements = new List<UiElementConfig>();
		public string levelTimerFormat;
		public Vector2 levelTimerPosition;
		public bool levelTimerOnlyBest;
		public bool levelTimerSaving;
		public string displayFormat;
		public Vector2 displayPosition;
		public Dictionary<string, Vector2> uiPositions = new Dictionary<string, Vector2>();
	}

	public class ModConfigData {
		public List<UiElementConfig> elements = new List<UiElementConfig>();
		public string levelTimerFormat = Formatter.LEVEL_TIME + " / " + Formatter.BEST_LEVEL_TIME;
		public Vector2 levelTimerPosition = new Vector2(0, Screen.height / 2f - 100);
		public bool levelTimerOnlyBest = false;
		public bool levelTimerSaving = true;

		public string displayFormat = Formatter.CLOCK;
		public Vector2 displayPosition = new Vector2(0, Screen.height / 2f - 50);

		public Dictionary<string, Vector2> uiPositions = new Dictionary<string, Vector2>();

	}

	public class Vector2Converter : JsonConverter<Vector2> {
		public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WriteEndObject();
		}

		public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var obj = JObject.Load(reader);
			return new Vector2(
				obj.TryGetValue("x", out var xTok) ? xTok.Value<float>() : 0f,
				obj.TryGetValue("y", out var yTok) ? yTok.Value<float>() : 0f
			);
		}
	}

	public static class ConfigService {
		static readonly string ConfigDir = Paths.ConfigPath;
		static readonly string ConfigPath = Path.Combine(ConfigDir, "InfoSkull.json");
		static readonly string PresetsDir = Path.Combine(Paths.PluginPath, "InfoSkullPresets");

		public static ModConfigData Data { get; private set; } = new ModConfigData();

		static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
			Formatting = Formatting.Indented,
			Converters = new List<JsonConverter> { new Vector2Converter() },
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		public static void LoadOrInitialize() {
			try {
				Directory.CreateDirectory(ConfigDir);
				Directory.CreateDirectory(PresetsDir);
				if (File.Exists(ConfigPath)) {
					var json = File.ReadAllText(ConfigPath);
					Data = JsonConvert.DeserializeObject<ModConfigData>(json, JsonSettings) ?? new ModConfigData();
				} else {
					Data = CreateDefault();
					Save();
				}
			} catch (Exception e) {
				Debug.LogError($"InfoSkull: Failed to load config: {e}");
				Data = CreateDefault();
				try { Save(); } catch (Exception se) { Debug.LogError($"InfoSkull: Failed to save default config: {se}"); }
			}
		}

		public static void Save() {
			try {
				Directory.CreateDirectory(ConfigDir);
				var json = JsonConvert.SerializeObject(Data, JsonSettings);
				File.WriteAllText(ConfigPath, json);
			} catch (Exception e) {
				Debug.LogError($"InfoSkull: Failed to save config: {e}");
			}
		}

		static ModConfigData CreateDefault() {
			var data = new ModConfigData();
			data.elements.Add(new UiElementConfig {
				id = Guid.NewGuid().ToString("N"),
				format = Formatter.ROACHES_BANKED_THIS_RUN + " / " + Formatter.ROACHES + " · " + Formatter.CLOCK,
				position = new Vector2(0, Screen.height / 2f - 50),
				rotationDegrees = 0,
				fontSize = 0f
			});
			return data;
		}

		public static PresetData SnapshotToPreset() {
			return new PresetData {
				elements = new List<UiElementConfig>(Data.elements.ConvertAll(e => new UiElementConfig {
					id = e.id,
					format = e.format,
					position = e.position,
					rotationDegrees = e.rotationDegrees,
					fontSize = e.fontSize
				})),
				levelTimerFormat = Data.levelTimerFormat,
				levelTimerPosition = Data.levelTimerPosition,
				levelTimerOnlyBest = Data.levelTimerOnlyBest,
				levelTimerSaving = Data.levelTimerSaving,
				displayFormat = Data.displayFormat,
				displayPosition = Data.displayPosition,
				uiPositions = new Dictionary<string, Vector2>(Data.uiPositions)
			};
		}

		public static void ApplyPreset(PresetData preset) {
			if (preset == null) return;
			Data.elements = new List<UiElementConfig>(preset.elements.ConvertAll(e => new UiElementConfig {
				id = string.IsNullOrEmpty(e.id) ? Guid.NewGuid().ToString("N") : e.id,
				format = e.format,
				position = e.position,
				rotationDegrees = e.rotationDegrees,
				fontSize = e.fontSize
			}));
			Data.levelTimerFormat = preset.levelTimerFormat;
			Data.levelTimerPosition = preset.levelTimerPosition;
			Data.levelTimerOnlyBest = preset.levelTimerOnlyBest;
			Data.levelTimerSaving = preset.levelTimerSaving;
			Data.displayFormat = preset.displayFormat;
			Data.displayPosition = preset.displayPosition;
			Data.uiPositions = new Dictionary<string, Vector2>(preset.uiPositions ?? new Dictionary<string, Vector2>());
			Save();
		}

		static string SanitizePresetName(string name) {
			if (string.IsNullOrWhiteSpace(name)) return "preset";
			var invalid = Path.GetInvalidFileNameChars();
			var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
			return string.IsNullOrWhiteSpace(cleaned) ? "preset" : cleaned;
		}

		public static string GetPresetsDirectory() {
			try { Directory.CreateDirectory(PresetsDir); } catch {}
			return PresetsDir;
		}

		public static void SaveCurrentAsPresetFile(string name) {
			var preset = SnapshotToPreset();
			SavePresetToFile(name, preset);
		}

		public static void SavePresetToFile(string name, PresetData preset) {
			if (preset == null) return;
			var dir = GetPresetsDirectory();
			var file = Path.Combine(dir, SanitizePresetName(name) + ".json");
			var json = JsonConvert.SerializeObject(preset, JsonSettings);
			File.WriteAllText(file, json);
		}

		public static PresetData LoadPresetFromFile(string name) {
			var dir = GetPresetsDirectory();
			var file = Path.Combine(dir, SanitizePresetName(name) + ".json");
			if (!File.Exists(file)) return null;
			var json = File.ReadAllText(file);
			return JsonConvert.DeserializeObject<PresetData>(json, JsonSettings);
		}

		public static string[] ListPresetNames() {
			var dir = GetPresetsDirectory();
			if (!Directory.Exists(dir)) return Array.Empty<string>();
			return Directory.GetFiles(dir, "*.json").Select(p => Path.GetFileNameWithoutExtension(p)).ToArray();
		}

		public static bool DeletePresetFile(string name) {
			var dir = GetPresetsDirectory();
			var file = Path.Combine(dir, SanitizePresetName(name) + ".json");
			if (!File.Exists(file)) return false;
			File.Delete(file);
			return true;
		}
	}
}

