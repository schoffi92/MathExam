using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

public partial class SummaryViewModel : ObservableObject
{
    private readonly Action _onBack;

    /// <param name="previousBest">For a timed challenge: the best earlier score with the same settings, if any.</param>
    public SummaryViewModel(GameSession session, string? saveError, int? previousBest, Action onBack)
    {
        _onBack = onBack;
        AnsweredText = string.Format(Strings.Summary_Answered, session.AnsweredCount);
        CorrectText = string.Format(Strings.Summary_Correct, session.CorrectCount);
        WrongText = string.Format(Strings.Summary_Wrong, session.WrongCount);
        AccuracyText = string.Format(Strings.Summary_Accuracy, session.AnsweredCount == 0
            ? "–"
            : ((double)session.CorrectCount / session.AnsweredCount).ToString("P0"));
        TimeText = string.Format(Strings.Common_Time, session.Elapsed.ToString(@"hh\:mm\:ss"));
        LevelText = session.IsAdaptive ? string.Format(Strings.Summary_Level, session.StartLevel, session.Level) : "";
        SaveError = saveError ?? "";

        TimeUpText = session.IsTimeUp ? Strings.Summary_TimeUp : "";
        if (session.TaskLimit is { } limit)
        {
            ScoreText = string.Format(Strings.Summary_Score, session.CorrectCount, limit);
            // What to practise: each wrong answer, with the task solved.
            Review = session.Mistakes
                .Select(m => string.Format(Strings.Summary_YourAnswer, m.Task.ToDisplayString(revealAnswer: true), m.Given))
                .ToList();
        }
        if (session.IsTimed)
        {
            BestText = session.CorrectCount > (previousBest ?? 0)
                ? string.Format(Strings.Summary_NewBest, session.CorrectCount)
                : previousBest is { } best ? string.Format(Strings.Summary_Best, best) : "";
        }
    }

    public string TimeUpText { get; }
    public string ScoreText { get; } = "";
    public IReadOnlyList<string> Review { get; } = [];
    public bool HasReview => Review.Count > 0;
    public string BestText { get; } = "";

    public string AnsweredText { get; }
    public string CorrectText { get; }
    public string WrongText { get; }
    public string AccuracyText { get; }
    public string TimeText { get; }
    public string LevelText { get; }
    public string SaveError { get; }

    [RelayCommand]
    private void BackToMenu() => _onBack();
}
