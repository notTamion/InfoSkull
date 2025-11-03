namespace InfoSkull;

public class Timer {
	public static float levelEnterTime;

	public static void completeRoom(M_Level level) {
		var currentTime = currentLevelTime();

		var statName = "info-skull-" + level.levelName + "-best-time";

		StatManager.sessionStats.UpdateStatistic(statName,
			currentTime, StatManager.Statistic.DataType.Float,
			StatManager.Statistic.ModType.Min, StatManager.Statistic.DisplayType.Time,
			StatManager.Statistic.ModType.Min);

		levelEnterTime = CL_GameManager.gMan.GetGameTime();
	}

	public static float currentLevelTime() {
		return CL_GameManager.gMan.GetGameTime() - levelEnterTime;
	}

	public static float bestLevelTime(M_Level level) {
		var statName = "info-skull-" + level.levelName + "-best-time";
		var stat = StatManager.saveData.gameStats.GetStatistic(statName);
		return stat.value == "" ? 0.0f : float.Parse(stat.value);
	}
}