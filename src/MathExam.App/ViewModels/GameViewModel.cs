using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

public enum AnswerState
{
    Answering,
    Correct,
    Wrong,
}

public partial class GameViewModel : ObservableObject
{
    private readonly GameSession _session;
    private readonly Action<GameSession> _onStop;
    private readonly DispatcherTimer _timer;

    public GameViewModel(GameSession session, Action<GameSession> onStop)
    {
        _session = session;
        _onStop = onStop;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => OnPropertyChanged(nameof(ElapsedText));
        _timer.Start();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnswered), nameof(ButtonText), nameof(FeedbackText), nameof(SolvedText),
        nameof(LevelText), nameof(LevelChangeText))]
    private AnswerState _state = AnswerState.Answering;

    [ObservableProperty]
    private string _answerText = "";

    public string Equation => _session.CurrentTask.ToDisplayString();
    public string TaskNumberText => $"Task #{_session.TaskNumber}";
    public string SolvedText => $"Solved: {_session.CorrectCount} / {_session.AnsweredCount}";
    public string StartedText => $"Started: {_session.StartedAt:HH:mm:ss}";
    public string ElapsedText => $"Elapsed: {_session.Elapsed.ToString(@"hh\:mm\:ss")}";
    public bool IsAnswered => State != AnswerState.Answering;
    public string ButtonText => IsAnswered ? "Next" : "Send";
    // Spelled out with a symbol so the result is clear without relying on colour.
    public string FeedbackText => State switch
    {
        AnswerState.Correct => "✓ Correct!",
        AnswerState.Wrong => $"✗ Correct answer: {_session.CurrentTask.Answer}",
        _ => "",
    };

    public string LevelText => _session.IsAdaptive ? $"Level {_session.Level} / {DifficultyAdjuster.MaxLevel}" : "";

    public string LevelChangeText => (IsAnswered ? _session.LastLevelChange : 0) switch
    {
        > 0 => "▲ Level up! The numbers get a little bigger.",
        < 0 => "▼ Level down. The numbers get a little smaller.",
        _ => "",
    };

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
            return;
        }

        // Accept the typographic minus as well as the ASCII one.
        var text = AnswerText.Trim().Replace('−', '-');
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var answer))
            return;

        State = _session.Submit(answer) ? AnswerState.Correct : AnswerState.Wrong;
    }

    [RelayCommand]
    private void Stop()
    {
        _timer.Stop();
        _session.Stop();
        _onStop(_session);
    }
}
