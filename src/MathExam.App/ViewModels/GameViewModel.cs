using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly GameSession _session;
    private readonly Action<GameSession> _onStop;
    private readonly DailyGoalProgress _goal;
    private readonly DispatcherTimer _timer;

    /// <param name="player">The profile playing, shown in the header; null for the guest.</param>
    /// <param name="goal">The player's daily goal and the tasks they answered earlier today.</param>
    public GameViewModel(GameSession session, Action<GameSession> onStop, string? player = null, DailyGoalProgress? goal = null)
    {
        _session = session;
        _onStop = onStop;
        PlayerName = player ?? "";
        _goal = goal ?? DailyGoalProgress.None;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => OnTick();
        _timer.Start();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnswered), nameof(IsCorrect), nameof(IsWrong), nameof(ButtonText),
        nameof(FeedbackText), nameof(ExplanationText), nameof(SolvedText), nameof(LevelText), nameof(LevelChangeText),
        nameof(ShowOperatorButtons), nameof(GoalText))]
    private AnswerState _state = AnswerState.Answering;

    [ObservableProperty]
    private string _answerText = "";

    public MathTask Equation => _session.CurrentTask;
    // A test counts towards its end: "Task 3 of 20".
    public string TaskNumberText => _session.TaskLimit is { } limit
        ? string.Format(Strings.FamilyGame_TaskInTurn, _session.TaskNumber, limit)
        : string.Format(Strings.Game_TaskNumber, _session.TaskNumber);
    public string PlayerName { get; }
    public string GoalText => _goal.Text(_session.AnsweredCount);
    // Hidden in a test, which reveals nothing about right and wrong until the end.
    public string SolvedText => _session.IsTest ? "" : string.Format(Strings.Game_Solved, _session.CorrectCount, _session.AnsweredCount);
    public string StartedText => string.Format(Strings.Game_Started, _session.StartedAt.ToString("HH:mm:ss"));

    /// <summary>Elapsed time, or in a timed challenge the time left.</summary>
    public string ClockText => _session.TimeLeft is { } left
        ? string.Format(Strings.Game_TimeLeft, left.ToString(@"m\:ss"))
        : string.Format(Strings.Game_Elapsed, _session.Elapsed.ToString(@"hh\:mm\:ss"));

    public bool IsAnswered => State != AnswerState.Answering;
    public bool IsCorrect => State == AnswerState.Correct;
    public bool IsWrong => State == AnswerState.Wrong;
    public string ButtonText => IsAnswered ? Strings.Game_Next : Strings.Game_Send;
    public string FeedbackText => Answers.Feedback(State, _session.CurrentTask);
    public string ExplanationText => Answers.Explanation(State, _session.CurrentTask);
    public string LevelText =>
        _session.IsAdaptive ? string.Format(Strings.Game_Level, _session.Level, DifficultyAdjuster.MaxLevel) : "";
    public string LevelChangeText => Answers.LevelChange(IsAnswered ? _session.LastLevelChange : 0);

    public bool ShowOperatorButtons => _session.CurrentTask.Hidden == HiddenPart.Operator && !IsAnswered;
    public IReadOnlyList<string> OperatorSymbols => Answers.OperatorSymbols;

    [RelayCommand]
    private void SubmitOrNext()
    {
        if (IsAnswered)
        {
            _session.NextTask();
            AnswerText = "";
            State = AnswerState.Answering;
            OnPropertyChanged(nameof(Equation));
            OnPropertyChanged(nameof(TaskNumberText));
            OnPropertyChanged(nameof(ShowOperatorButtons));
            return;
        }

        var result = Answers.Submit(AnswerText, _session.CurrentTask, _session.Submit, _session.SubmitOperator);
        if (result is not { } correct)
            return;

        if (!_session.IsTest)
        {
            State = correct ? AnswerState.Correct : AnswerState.Wrong;
            return;
        }

        // A test shows no feedback until the end: straight on to the next task, or to the summary.
        if (_session.IsTestComplete)
        {
            Stop();
            return;
        }
        _session.NextTask();
        AnswerText = "";
        OnPropertyChanged(nameof(Equation));
        OnPropertyChanged(nameof(TaskNumberText));
        OnPropertyChanged(nameof(ShowOperatorButtons));
        OnPropertyChanged(nameof(GoalText));
    }

    /// <summary>An operator button of a missing-operator task: fills in the symbol and sends it.</summary>
    [RelayCommand]
    private void PickOperator(string symbol)
    {
        if (IsAnswered)
            return;
        AnswerText = symbol;
        SubmitOrNext();
    }

    [RelayCommand]
    private void Stop()
    {
        _timer.Stop();
        _session.Stop();
        _onStop(_session);
    }

    private void OnTick()
    {
        OnPropertyChanged(nameof(ClockText));
        if (_session.IsTimeUp)
            Stop();
    }
}
