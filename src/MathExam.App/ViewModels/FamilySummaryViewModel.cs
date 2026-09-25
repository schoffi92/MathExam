using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One line of the final ranking.</summary>
public sealed record RankingRow(string Place, string Name, string Score, string Accuracy, string Level);

public partial class FamilySummaryViewModel : ObservableObject
{
    private readonly Action _onPlayAgain;
    private readonly Action _onBack;

    public FamilySummaryViewModel(FamilyGame game, string? saveError, Action onPlayAgain, Action onBack)
    {
        _onPlayAgain = onPlayAgain;
        _onBack = onBack;

        var winners = game.Winners.Select(p => p.Name).ToList();
        WinnerText = winners.Count switch
        {
            0 => "Nobody scored this time. Try again!",
            1 => $"{winners[0]} wins!",
            _ => $"It's a tie: {string.Join(" and ", winners)}!",
        };

        // Players with the same score and accuracy share a place.
        var rows = new List<RankingRow>();
        var place = 0;
        (int, double)? previous = null;
        var ranking = game.Ranking;
        for (var i = 0; i < ranking.Count; i++)
        {
            var s = ranking[i].Session;
            var accuracy = s.AnsweredCount == 0 ? 0 : (double)s.CorrectCount / s.AnsweredCount;
            if (previous != (s.CorrectCount, accuracy))
                place = i + 1;
            previous = (s.CorrectCount, accuracy);
            rows.Add(new RankingRow(
                $"{place}.",
                ranking[i].Name,
                $"{s.CorrectCount} / {s.AnsweredCount} correct",
                s.AnsweredCount == 0 ? "–" : $"{accuracy:P0}",
                $"Level {s.StartLevel} → {s.Level}"));
        }
        Rows = rows;

        TimeText = $"Time: {game.Players[0].Session.Elapsed.ToString(@"hh\:mm\:ss")}";
        SaveError = saveError ?? "";
    }

    public string WinnerText { get; }
    public IReadOnlyList<RankingRow> Rows { get; }
    public string TimeText { get; }
    public string SaveError { get; }

    [RelayCommand]
    private void PlayAgain() => _onPlayAgain();

    [RelayCommand]
    private void BackToMenu() => _onBack();
}
