using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

public partial class SummaryViewModel : ObservableObject
{
    private readonly Action _onBack;

    public SummaryViewModel(GameSession session, Action onBack)
    {
        _onBack = onBack;
        AnsweredText = $"Answered: {session.AnsweredCount}";
        CorrectText = $"Correct: {session.CorrectCount}";
        WrongText = $"Wrong: {session.WrongCount}";
        AccuracyText = session.AnsweredCount == 0
            ? "Accuracy: –"
            : $"Accuracy: {(double)session.CorrectCount / session.AnsweredCount:P0}";
        TimeText = $"Time: {session.Elapsed.ToString(@"hh\:mm\:ss")}";
    }

    public string AnsweredText { get; }
    public string CorrectText { get; }
    public string WrongText { get; }
    public string AccuracyText { get; }
    public string TimeText { get; }

    [RelayCommand]
    private void BackToMenu() => _onBack();
}
