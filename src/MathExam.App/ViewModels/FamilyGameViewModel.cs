using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One player's box on the family scoreboard.</summary>
public sealed record ScoreRow(string Name, string Score, string Level, bool IsCurrent);

public partial class FamilyGameViewModel : ObservableObject
{
    private readonly FamilyGame _game;
    private readonly Action<FamilyGame> _onStop;
    private readonly DispatcherTimer _timer;

    public FamilyGameViewModel(FamilyGame game, Action<FamilyGame> onStop)
    {
        _game = game;
        _onStop = onStop;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => OnPropertyChanged(nameof(ElapsedText));
        _timer.Start();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnswered), nameof(IsCorrect), nameof(IsWrong), nameof(ButtonText),
        nameof(FeedbackText), nameof(LevelChangeText), nameof(Scoreboard))]
    private AnswerState _state = AnswerState.Answering;

    [ObservableProperty]
    private string _answerText = "";

    private GameSession Session => _game.Current.Session;

    public string TurnText => string.Format(Strings.FamilyGame_Turn, _game.Current.Name);
    public string RoundText => string.Format(Strings.FamilyGame_Round, _game.Round);
    public string ElapsedText => string.Format(Strings.Game_Elapsed, Session.Elapsed.ToString(@"hh\:mm\:ss"));
    public MathTask Equation => Session.CurrentTask;
    public bool IsAnswered => State != AnswerState.Answering;
    public bool IsCorrect => State == AnswerState.Correct;
    public bool IsWrong => State == AnswerState.Wrong;
    public string ButtonText => IsAnswered ? Strings.FamilyGame_NextPlayer : Strings.Game_Send;
    public string FeedbackText => Answers.Feedback(State, Session.CurrentTask.Answer);
    public string LevelChangeText => Answers.LevelChange(IsAnswered ? Session.LastLevelChange : 0);

    public IReadOnlyList<ScoreRow> Scoreboard => _game.Players
        .Select(p => new ScoreRow(
            p.Name,
            string.Format(Strings.Family_Score, p.Session.CorrectCount, p.Session.AnsweredCount),
            string.Format(Strings.Family_Level, p.Session.Level),
            p == _game.Current))
        .ToList();

    [RelayCommand]
    private void SubmitOrNext()
    {
        if (IsAnswered)
        {
            _game.NextTurn();
            AnswerText = "";
            State = AnswerState.Answering;
            OnPropertyChanged(nameof(TurnText));
            OnPropertyChanged(nameof(RoundText));
            OnPropertyChanged(nameof(Equation));
            return;
        }

        if (!Answers.TryParse(AnswerText, out var answer))
            return;

        State = _game.Submit(answer) ? AnswerState.Correct : AnswerState.Wrong;
    }

    [RelayCommand]
    private void Stop()
    {
        _timer.Stop();
        _game.Stop();
        _onStop(_game);
    }
}
