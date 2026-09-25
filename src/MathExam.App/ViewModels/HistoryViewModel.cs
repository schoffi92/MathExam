using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One history table row, pre-formatted for display.</summary>
public sealed record HistoryRow(
    string Date, string Range, string Operations, int Answered, int Correct, string Accuracy, string Time, string Level);

public partial class HistoryViewModel : ObservableObject
{
    private readonly Action _onBack;

    public HistoryViewModel(IReadOnlyList<SessionRecord> records, string? loadError, Action onBack)
    {
        _onBack = onBack;
        LoadError = loadError ?? "";

        Rows = records
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new HistoryRow(
                r.StartedAt.ToString("yyyy-MM-dd HH:mm"),
                $"{r.Min} – {r.Max}",
                string.Join(" ", r.Operations.Select(o => o.Symbol())),
                r.Answered,
                r.Correct,
                $"{r.Accuracy:P0}",
                r.Duration.ToString(@"hh\:mm\:ss"),
                r.IsAdaptive ? $"{r.StartLevel} → {r.EndLevel}" : "–"))
            .ToList();

        var answered = records.Sum(r => r.Answered);
        var correct = records.Sum(r => r.Correct);
        var time = TimeSpan.FromTicks(records.Sum(r => r.Duration.Ticks));
        TotalsText = records.Count == 0
            ? ""
            : $"{records.Count} {(records.Count == 1 ? "game" : "games")} · {answered} tasks · {(double)correct / Math.Max(answered, 1):P0} correct · " +
              $"{(int)time.TotalHours:00}:{time:mm\\:ss} total";
    }

    public IReadOnlyList<HistoryRow> Rows { get; }
    public bool IsEmpty => Rows.Count == 0;
    public string TotalsText { get; }
    public string LoadError { get; }

    [RelayCommand]
    private void BackToMenu() => _onBack();
}
