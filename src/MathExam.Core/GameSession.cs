using System.Diagnostics;

namespace MathExam.Core;

/// <summary>An endless game: tasks keep coming until <see cref="Stop"/> is called.</summary>
public sealed class GameSession
{
    private readonly TaskGenerator _generator;
    private readonly Stopwatch _stopwatch = new();

    public GameSession(GameSettings settings, TaskGenerator? generator = null)
    {
        Settings = settings;
        _generator = generator ?? new TaskGenerator();
        StartedAt = DateTime.Now;
        _stopwatch.Start();
        CurrentTask = _generator.Next(settings);
        TaskNumber = 1;
    }

    public GameSettings Settings { get; }
    public DateTime StartedAt { get; }
    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public bool IsRunning => _stopwatch.IsRunning;

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
        return correct;
    }

    public void NextTask()
    {
        CurrentTask = _generator.Next(Settings);
        TaskNumber++;
        IsCurrentAnswered = false;
    }

    public void Stop() => _stopwatch.Stop();
}
