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
    [NotifyPropertyChangedFor(nameof(IsAnswered), nameof(IsCorrect), nameof(IsWrong), nameof(ButtonText),
        nameof(FeedbackText), nameof(SolvedText), nameof(LevelText), nameof(LevelChangeText))]
    private AnswerState _state = AnswerState.Answering;

    [ObservableProperty]
    private string _answerText = "";

    public string Equation => _session.CurrentTask.ToDisplayString();
    public string TaskNumberText => string.Format(Strings.Game_TaskNumber, _session.TaskNumber);
    public string SolvedText => string.Format(Strings.Game_Solved, _session.CorrectCount, _session.AnsweredCount);
    public string StartedText => string.Format(Strings.Game_Started, _session.StartedAt.ToString("HH:mm:ss"));
    public string ElapsedText => string.Format(Strings.Game_Elapsed, _session.Elapsed.ToString(@"hh\:mm\:ss"));
    public bool IsAnswered => State != AnswerState.Answering;
    public bool IsCorrect => State == AnswerState.Correct;
    public bool IsWrong => State == AnswerState.Wrong;
    public string ButtonText => IsAnswered ? Strings.Game_Next : Strings.Game_Send;
    public string FeedbackText => Answers.Feedback(State, _session.CurrentTask.Answer);
    public string LevelText =>
        _session.IsAdaptive ? string.Format(Strings.Game_Level, _session.Level, DifficultyAdjuster.MaxLevel) : "";
    public string LevelChangeText => Answers.LevelChange(IsAnswered ? _session.LastLevelChange : 0);

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

        if (!Answers.TryParse(AnswerText, out var answer))
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
