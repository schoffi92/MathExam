using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One history table row, pre-formatted for display.</summary>
public sealed record HistoryRow(
    string Date, string Player, string Tasks, string Score, string Accuracy, string Time, string Level);

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
                r.Player ?? Strings.History_Solo,
                $"{r.Min}–{r.Max}  {string.Join(" ", r.Operations.Select(o => o.Symbol()))}",
                $"{r.Correct} / {r.Answered}",
                r.Accuracy.ToString("P0"),
                r.Duration.TotalHours >= 1 ? r.Duration.ToString(@"h\:mm\:ss") : r.Duration.ToString(@"m\:ss"),
                r.IsAdaptive ? $"{r.StartLevel} → {r.EndLevel}" : "–"))
            .ToList();

        var totals = HistoryTotals.From(records);
        TotalsText = records.Count == 0
            ? ""
            : string.Format(Strings.History_Totals,
                string.Format(totals.Games == 1 ? Strings.History_GameOne : Strings.History_GameMany, totals.Games),
                totals.Tasks,
                totals.Accuracy.ToString("P0"),
                $"{(int)totals.Time.TotalHours:00}:{totals.Time:mm\\:ss}");
    }

    public IReadOnlyList<HistoryRow> Rows { get; }
    public bool IsEmpty => Rows.Count == 0;
    public string TotalsText { get; }
    public string LoadError { get; }

    [RelayCommand]
    private void BackToMenu() => _onBack();
}
