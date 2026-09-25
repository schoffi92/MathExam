using System.Diagnostics;

namespace MathExam.Core;

/// <summary>An endless game: tasks keep coming until <see cref="Stop"/> is called.</summary>
public sealed class GameSession
{
    private readonly TaskGenerator _generator;
    private readonly Stopwatch _stopwatch = new();

    /// <param name="difficulty">When given, the game is adaptive: tasks use the adjuster's current range.</param>
    /// <param name="startedAt">Start time; family games pass one shared time so their players' records belong together.</param>
    public GameSession(GameSettings settings, TaskGenerator? generator = null, DifficultyAdjuster? difficulty = null,
        DateTime? startedAt = null)
    {
        Settings = settings;
        _generator = generator ?? new TaskGenerator();
        Difficulty = difficulty;
        StartLevel = difficulty?.Level;
        StartedAt = startedAt ?? DateTime.Now;
        _stopwatch.Start();
        CurrentTask = _generator.Next(TaskSettings);
        TaskNumber = 1;
    }

    public GameSettings Settings { get; }
    public DateTime StartedAt { get; }
    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public bool IsRunning => _stopwatch.IsRunning;

    public DifficultyAdjuster? Difficulty { get; }
    public bool IsAdaptive => Difficulty is not null;
    public int? StartLevel { get; }
    public int? Level => Difficulty?.Level;

    /// <summary>Level change caused by the last answer: +1, -1 or 0.</summary>
    public int LastLevelChange { get; private set; }

    /// <summary>The settings tasks are generated from: the full settings, or the adaptive range.</summary>
    public GameSettings TaskSettings => Difficulty?.Apply(Settings) ?? Settings;

    public MathTask CurrentTask { get; private set; }
    public int TaskNumber { get; private set; }
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int AnsweredCount => CorrectCount + WrongCount;

    /// <summary>True once the current task has been answered; call <see cref="NextTask"/> to continue.</summary>
    public bool IsCurrentAnswered { get; private set; }

    public bool Submit(long answer)
    {
        if (IsCurrentAnswered)
            throw new InvalidOperationException("The current task has already been answered.");

        IsCurrentAnswered = true;
        var correct = CurrentTask.IsCorrect(answer);
        if (correct)
            CorrectCount++;
        else
            WrongCount++;
        LastLevelChange = Difficulty?.RecordAnswer(correct) ?? 0;
        return correct;
    }

    public void NextTask()
    {
        CurrentTask = _generator.Next(TaskSettings);
        TaskNumber++;
        IsCurrentAnswered = false;
        LastLevelChange = 0;
    }

    public void Stop() => _stopwatch.Stop();
}
