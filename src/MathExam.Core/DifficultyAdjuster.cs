namespace MathExam.Core;

/// <summary>
/// Adaptive difficulty. The level rises after a streak of correct answers and falls after
/// consecutive mistakes. Lower levels narrow the number range towards its easiest value
/// (the number closest to zero), level <see cref="MaxLevel"/> uses the full range.
/// </summary>
public sealed class DifficultyAdjuster
{
    public const int MinLevel = 1;
    public const int MaxLevel = 10;
    public const int CorrectStreakToLevelUp = 3;
    public const int WrongStreakToLevelDown = 2;

    private int _correctStreak;
    private int _wrongStreak;

    public DifficultyAdjuster(int startLevel = MinLevel)
    {
        Level = Math.Clamp(startLevel, MinLevel, MaxLevel);
    }

    public int Level { get; private set; }

    /// <summary>Records an answer and returns the level change: +1, -1 or 0.</summary>
    public int RecordAnswer(bool correct)
    {
        if (correct)
        {
            _wrongStreak = 0;
            if (++_correctStreak < CorrectStreakToLevelUp)
                return 0;
            _correctStreak = 0;
            if (Level == MaxLevel)
                return 0;
            Level++;
            return 1;
        }

        _correctStreak = 0;
        if (++_wrongStreak < WrongStreakToLevelDown)
            return 0;
        _wrongStreak = 0;
        if (Level == MinLevel)
            return 0;
        Level--;
        return -1;
    }

    public GameSettings Apply(GameSettings settings) => ForLevel(settings, Level);

    /// <summary>
    /// The range grows outwards from the easiest number proportionally to the level, rounding up
    /// so every level above the anchor offers at least one more number. A valid range therefore
    /// stays valid (e.g. division always keeps a non-zero number).
    /// </summary>
    public static GameSettings ForLevel(GameSettings settings, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        long anchor = Math.Clamp(0, settings.Min, settings.Max);
        var min = anchor - CeilingPortion(anchor - settings.Min, level);
        var max = anchor + CeilingPortion(settings.Max - anchor, level);
        return new GameSettings((int)min, (int)max, settings.Operations);
    }

    private static long CeilingPortion(long span, int level) => (span * level + MaxLevel - 1) / MaxLevel;
}
