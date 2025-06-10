using System;
using System.Collections.Generic;
using System.Linq;

namespace InfoSkull.utils;

public class StatsUtil {
	public static void wipe(Func<KeyValuePair<string, string>, bool> predicate) {
		StatManager.saveData.gameStats.statsDictionary = StatManager.saveData.gameStats.statsDictionary
			.Where(pair => !predicate.Invoke(new KeyValuePair<string, string>(pair.Key, pair.Value.value)))
			.ToDictionary(i => i.Key, i => i.Value);
		StatManager.saveData.gameStats.statistics = StatManager.saveData.gameStats.statistics
			.Where(stat => !predicate.Invoke(new KeyValuePair<string, string>(stat.id, stat.value)))
			.ToList();
		StatManager.sessionStats.statsDictionary = StatManager.saveData.gameStats.statsDictionary
			.Where(pair => !predicate.Invoke(new KeyValuePair<string, string>(pair.Key, pair.Value.value)))
			.ToDictionary(i => i.Key, i => i.Value);
		StatManager.sessionStats.statistics = StatManager.saveData.gameStats.statistics
			.Where(stat => !predicate.Invoke(new KeyValuePair<string, string>(stat.id, stat.value)))
			.ToList();
		foreach (var saveDataGameMode in StatManager.saveData.gameModes) {
			saveDataGameMode.stats.statsDictionary = StatManager.saveData.gameStats.statsDictionary
				.Where(pair => !predicate.Invoke(new KeyValuePair<string, string>(pair.Key, pair.Value.value)))
				.ToDictionary(i => i.Key, i => i.Value);
			saveDataGameMode.stats.statistics = StatManager.saveData.gameStats.statistics
				.Where(stat => !predicate.Invoke(new KeyValuePair<string, string>(stat.id, stat.value)))
				.ToList();
		}
	}
}