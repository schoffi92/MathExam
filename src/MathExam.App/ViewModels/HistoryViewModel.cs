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
    private readonly IReadOnlyList<SessionRecord> _records;
    private readonly Action _onBack;
    private readonly Func<IReadOnlyList<SessionRecord>, Task<string?>> _onExport;

    /// <param name="onExport">Saves the records as CSV; returns a message to show, or null if cancelled.</param>
    public HistoryViewModel(IReadOnlyList<SessionRecord> records, string? loadError, Action onBack,
        Func<IReadOnlyList<SessionRecord>, Task<string?>> onExport)
    {
        _records = records;
        _onBack = onBack;
        _onExport = onExport;
        LoadError = loadError ?? "";

        Rows = records
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new HistoryRow(
                r.StartedAt.ToString("yyyy-MM-dd HH:mm"),
                r.Player ?? Strings.History_Solo,
                DescribeTasks(r),
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

        // "+ 95 % · × 70 %": where the player is strong or weak.
        ByOperationText = totals.ByOperation.Count == 0
            ? ""
            : string.Format(Strings.History_ByOperation, string.Join("  ·  ",
                totals.ByOperation.Select(pair => $"{pair.Key.Symbol()} {pair.Value.Accuracy:P0}")));
    }

    public string ByOperationText { get; }

    /// <summary>The result of the last export.</summary>
    [ObservableProperty]
    private string _statusText = "";

    public IReadOnlyList<HistoryRow> Rows { get; }
    public bool IsEmpty => Rows.Count == 0;
    public string TotalsText { get; }
    public string LoadError { get; }

    [RelayCommand]
    private void BackToMenu() => _onBack();

    [RelayCommand(CanExecute = nameof(HasRecords))]
    private async Task Export()
    {
        if (await _onExport(_records) is { } message)
            StatusText = message;
    }

    private bool HasRecords() => _records.Count > 0;

    /// <summary>"1–10  + − × ÷", marked when the game used fractions, decimals or missing operators.</summary>
    private static string DescribeTasks(SessionRecord r)
    {
        var text = $"{r.Min}–{r.Max}  {string.Join(" ", r.Operations.Select(o => o.Symbol()))}";
        if (r.Numbers == NumberKind.Fraction)
            text += "  (1/2)";
        else if (r.Numbers == NumberKind.Decimal)
            text += "  (0.1)";
        if (r.MissingOperator)
            text += "  (?)";
        return text;
    }
}
