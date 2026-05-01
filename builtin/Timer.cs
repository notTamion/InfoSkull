namespace InfoSkull;

public class Timer
{
	public static float levelEnterTime;
	public static float realTime;

	public static void completeRoom(M_Level level)
	{
		var currentTime = currentLevelTime();

		var statName = "info-skull-" + level.levelName + "-best-time";

		StatManager.sessionStats.UpdateStatistic(statName,
			currentTime, StatManager.Statistic.DataType.Float,
			StatManager.Statistic.ModType.Min, StatManager.Statistic.DisplayType.Time,
			StatManager.Statistic.ModType.Min);

		levelEnterTime = CL_GameManager.gMan.GetGameTime();
	}

	public static float currentLevelTime()
	{
		return CL_GameManager.gMan.GetGameTime() - levelEnterTime;
	}

	public static float bestLevelTime(M_Level level)
	{
		var statName = "info-skull-" + level.levelName + "-best-time";
		var stat = StatManager.sessionStats.GetStatistic(statName).value;
		var saveStat = StatManager.saveData.gameStats.GetStatistic(statName).value;
		if (stat == "")
		{
			return saveStat == "" ? 0.0f : float.Parse(saveStat);
		}
		if (saveStat == "")
		{
			return float.Parse(stat);
		}
		return float.Parse(stat) > float.Parse(saveStat) ? float.Parse(saveStat) : float.Parse(stat);
	}
}
