namespace MathExam.Core;

/// <summary>Right and wrong answers for one operation.</summary>
public sealed record OperationStats(int Correct, int Wrong)
{
    public int Answered => Correct + Wrong;
    public double Accuracy => Answered == 0 ? 0 : (double)Correct / Answered;
}

/// <summary>
/// A game: tasks keep coming until <see cref="Stop"/> is called, or, with a time limit, until the time is up.
/// </summary>
public sealed class GameSession
{
    private readonly TaskGenerator _generator;
    private readonly TimeProvider _clock;
    private readonly long _startTimestamp;
    private readonly Dictionary<Operation, OperationStats> _byOperation = [];
    private TimeSpan? _stoppedAt;

    /// <param name="difficulty">When given, the game is adaptive: tasks use the adjuster's current range.</param>
    /// <param name="startedAt">Start time; family games pass one shared time so their players' records belong together.</param>
    /// <param name="timeLimit">For a timed challenge: the game ends when this much time has passed.</param>
    /// <param name="clock">Measures the elapsed time; tests pass a controllable one.</param>
    public GameSession(GameSettings settings, TaskGenerator? generator = null, DifficultyAdjuster? difficulty = null,
        DateTime? startedAt = null, TimeSpan? timeLimit = null, TimeProvider? clock = null)
    {
        Settings = settings;
        _generator = generator ?? new TaskGenerator();
        Difficulty = difficulty;
        StartLevel = difficulty?.Level;
        StartedAt = startedAt ?? DateTime.Now;
        TimeLimit = timeLimit;
        _clock = clock ?? TimeProvider.System;
        _startTimestamp = _clock.GetTimestamp();
        CurrentTask = _generator.Next(TaskSettings);
        TaskNumber = 1;
    }

    public GameSettings Settings { get; }
    public DateTime StartedAt { get; }

    /// <summary>Time played; a timed game never reports more than its limit.</summary>
    public TimeSpan Elapsed
    {
        get
        {
            var elapsed = _stoppedAt ?? _clock.GetElapsedTime(_startTimestamp);
            return TimeLimit is { } limit && elapsed > limit ? limit : elapsed;
        }
    }

    public bool IsRunning => _stoppedAt is null;

    public TimeSpan? TimeLimit { get; }
    public bool IsTimed => TimeLimit is not null;
    public TimeSpan? TimeLeft => TimeLimit - Elapsed;
    public bool IsTimeUp => TimeLimit is { } limit && Elapsed >= limit;

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

    /// <summary>Right and wrong answers per operation, for the operations that were asked.</summary>
    public IReadOnlyDictionary<Operation, OperationStats> ByOperation => _byOperation;

    /// <summary>True once the current task has been answered; call <see cref="NextTask"/> to continue.</summary>
    public bool IsCurrentAnswered { get; private set; }

    public bool Submit(long answer) => Record(CurrentTask.IsCorrect(answer));

    public bool Submit(Rational answer) => Record(CurrentTask.IsCorrect(answer));

    /// <summary>Answers a missing-operator task.</summary>
    public bool SubmitOperator(Operation answer) => Record(CurrentTask.IsCorrect(answer));

    public void NextTask()
    {
        CurrentTask = _generator.Next(TaskSettings);
        TaskNumber++;
        IsCurrentAnswered = false;
        LastLevelChange = 0;
    }

    public void Stop() => _stoppedAt ??= Elapsed;

    private bool Record(bool correct)
    {
        if (IsCurrentAnswered)
            throw new InvalidOperationException("The current task has already been answered.");

        IsCurrentAnswered = true;
        if (correct)
            CorrectCount++;
        else
            WrongCount++;

        var op = CurrentTask.Op;
        var stats = _byOperation.GetValueOrDefault(op) ?? new OperationStats(0, 0);
        _byOperation[op] = correct ? stats with { Correct = stats.Correct + 1 } : stats with { Wrong = stats.Wrong + 1 };

        LastLevelChange = Difficulty?.RecordAnswer(correct) ?? 0;
        return correct;
    }
}
