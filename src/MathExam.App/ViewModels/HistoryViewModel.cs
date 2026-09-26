using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One history table row, pre-formatted for display.</summary>
public sealed record HistoryRow(
    string Date, string Player, string Game, string Tasks, string Score, string Accuracy, string Time, string Level);

/// <summary>One row of the Players tab: a player's totals, e.g. for a teacher's overview of a class.</summary>
public sealed record PlayerOverviewRow(
    string Player, string Games, string Tasks, string Accuracy, string Time, string Today, string LastPlayed);

/// <summary>A choice in the "Show" list: all players, or one (the guest has a null name).</summary>
public sealed record HistoryFilter(bool All, string? Name)
{
    public string DisplayName => All ? Strings.History_AllPlayers : Name ?? Strings.Menu_Guest;
}

public partial class HistoryViewModel : ObservableObject
{
    private readonly IReadOnlyList<SessionRecord> _records;
    private readonly Action _onBack;
    private readonly Func<IReadOnlyList<SessionRecord>, Task<string?>> _onExport;

    /// <param name="onExport">Saves the records as CSV; returns a message to show, or null if cancelled.</param>
    public HistoryViewModel(IReadOnlyList<SessionRecord> records, ProfileList profiles, string? loadError, Action onBack,
        Func<IReadOnlyList<SessionRecord>, Task<string?>> onExport)
    {
        _records = records;
        _onBack = onBack;
        _onExport = onExport;
        LoadError = loadError ?? "";

        var today = DateOnly.FromDateTime(DateTime.Now);
        var overview = PlayerStats.Overview(records, profiles, today);
        Players = overview.Select(p => new PlayerOverviewRow(
                p.Name ?? Strings.Menu_Guest,
                p.Games.ToString(),
                p.Tasks.ToString(),
                p.Tasks == 0 ? "–" : p.Accuracy.ToString("P0"),
                FormatTotalTime(p.Time),
                p.DailyGoal > 0 ? $"{p.TasksToday} / {p.DailyGoal}" : p.TasksToday.ToString(),
                p.LastPlayed?.ToString("yyyy-MM-dd HH:mm") ?? "–"))
            .ToList();

        Filters = [new HistoryFilter(true, null), .. overview.Select(p => new HistoryFilter(false, p.Name))];
        _filter = Filters[0];
        Refresh();
    }

    public IReadOnlyList<HistoryFilter> Filters { get; }
    public IReadOnlyList<PlayerOverviewRow> Players { get; }

    [ObservableProperty]
    private HistoryFilter _filter;

    [ObservableProperty]
    private IReadOnlyList<HistoryRow> _rows = [];

    [ObservableProperty]
    private string _totalsText = "";

    [ObservableProperty]
    private string _byOperationText = "";

    /// <summary>The result of the last export.</summary>
    [ObservableProperty]
    private string _statusText = "";

    public bool IsEmpty => Rows.Count == 0;
    public string LoadError { get; }

    partial void OnFilterChanged(HistoryFilter value) => Refresh();

    private IReadOnlyList<SessionRecord> Shown =>
        Filter.All ? _records : _records.Where(r => PlayerStats.IsPlayedBy(r, Filter.Name)).ToList();

    private void Refresh()
    {
        var records = Shown;
        Rows = records
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new HistoryRow(
                r.StartedAt.ToString("yyyy-MM-dd HH:mm"),
                r.Who ?? Strings.Menu_Guest,
                DescribeMode(r),
                DescribeTasks(r),
                $"{r.Correct} / {r.Answered}",
                r.Accuracy.ToString("P0"),
                r.Duration.TotalHours >= 1 ? r.Duration.ToString(@"h\:mm\:ss") : r.Duration.ToString(@"m\:ss"),
                r.IsAdaptive ? $"{r.StartLevel} → {r.EndLevel}" : "–"))
            .ToList();
        OnPropertyChanged(nameof(IsEmpty));

        var totals = HistoryTotals.From(records);
        TotalsText = records.Count == 0
            ? ""
            : string.Format(Strings.History_Totals,
                string.Format(totals.Games == 1 ? Strings.History_GameOne : Strings.History_GameMany, totals.Games),
                totals.Tasks,
                totals.Accuracy.ToString("P0"),
                FormatTotalTime(totals.Time));

        // "+ 95 % · × 70 %": where the player is strong or weak.
        ByOperationText = totals.ByOperation.Count == 0
            ? ""
            : string.Format(Strings.History_ByOperation, string.Join("  ·  ",
                totals.ByOperation.Select(pair => $"{pair.Key.Symbol()} {pair.Value.Accuracy:P0}")));
        ExportCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BackToMenu() => _onBack();

    /// <summary>Exports what is shown: every player's games, or the chosen player's.</summary>
    [RelayCommand(CanExecute = nameof(HasRecords))]
    private async Task Export()
    {
        if (await _onExport(Shown) is { } message)
            StatusText = message;
    }

    private bool HasRecords() => Rows.Count > 0;

    private static string FormatTotalTime(TimeSpan time) => $"{(int)time.TotalHours:00}:{time:mm\\:ss}";

    private static string DescribeMode(SessionRecord r) => r.Mode switch
    {
        GameMode.Family => Strings.Menu_Family,
        GameMode.Timed => Strings.Mode_TimedShort,
        GameMode.Test => $"{Strings.Mode_TestShort} ({r.TaskLimit})",
        _ => Strings.Mode_Practice,
    };

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
