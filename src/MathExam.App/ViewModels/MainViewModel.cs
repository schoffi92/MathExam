using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Navigates between the menu, game, summary, history and family screens.</summary>
public partial class MainViewModel : ObservableObject
{
    /// <summary>How long a timed challenge lasts.</summary>
    public static readonly TimeSpan ChallengeTime = TimeSpan.FromMinutes(1);

    private readonly IFileSaver _fileSaver;
    private readonly MenuViewModel _menu;
    private readonly FamilySetupViewModel _familySetup;
    private readonly HistoryStore _history = new(HistoryStore.DefaultPath);

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(IFileSaver fileSaver)
    {
        _fileSaver = fileSaver;
        Display = new DisplayViewModel(new PreferencesStore(PreferencesStore.DefaultPath));
        // The menu and family setup are reused so settings and player names survive a game.
        _menu = new MenuViewModel(Display, StartGame, ShowFamilySetup, ShowHistory, SaveWorksheetAsync);
        _familySetup = new FamilySetupViewModel(StartFamilyGame, ShowMenu);
        _currentViewModel = _menu;
    }

    public DisplayViewModel Display { get; }

    private void StartGame(GameSettings settings, bool adaptive, bool timed)
    {
        DifficultyAdjuster? difficulty = null;
        if (adaptive)
            difficulty = new DifficultyAdjuster(HistoryStore.ResumeLevel(LoadHistorySafe(), settings));

        var session = new GameSession(settings, difficulty: difficulty, timeLimit: timed ? ChallengeTime : null);
        CurrentViewModel = new GameViewModel(session, ShowSummary);
    }

    private void ShowSummary(GameSession session)
    {
        var record = SessionRecord.FromSession(session);
        // Read before saving, so this game is compared with the earlier ones.
        var best = session.IsTimed ? HistoryStore.PersonalBest(LoadHistorySafe(), record) : null;
        var saveError = session.AnsweredCount > 0 ? SaveToHistory([record]) : null;
        CurrentViewModel = new SummaryViewModel(session, saveError, best, ShowMenu);
    }

    private void ShowFamilySetup(GameSettings settings)
    {
        _familySetup.Open(settings);
        CurrentViewModel = _familySetup;
    }

    /// <summary>Family games are always adaptive: every player resumes at their own last level.</summary>
    private void StartFamilyGame(GameSettings settings, IReadOnlyList<string> names, int tasksPerTurn)
    {
        var history = LoadHistorySafe();
        var players = names.Select(name => (name, HistoryStore.ResumeLevel(history, settings, name)));
        CurrentViewModel = new FamilyGameViewModel(new FamilyGame(settings, players, tasksPerTurn: tasksPerTurn), game => ShowFamilySummary(game, names));
    }

    private void ShowFamilySummary(FamilyGame game, IReadOnlyList<string> names)
    {
        var records = game.Players
            .Where(p => p.Session.AnsweredCount > 0)
            .Select(p => SessionRecord.FromSession(p.Session, p.Name))
            .ToList();
        var saveError = records.Count > 0 ? SaveToHistory(records) : null;
        CurrentViewModel = new FamilySummaryViewModel(game, saveError, () => StartFamilyGame(game.Settings, names, game.TasksPerTurn), ShowMenu);
    }

    private void ShowHistory()
    {
        try
        {
            CurrentViewModel = new HistoryViewModel(_history.Load(), null, ShowMenu, ExportHistoryAsync);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            CurrentViewModel = new HistoryViewModel([], string.Format(Strings.History_LoadError, ex.Message), ShowMenu,
                ExportHistoryAsync);
        }
    }

    /// <summary>Saves the records and returns an error message for the summary screen, or null on success.</summary>
    private string? SaveToHistory(IEnumerable<SessionRecord> records)
    {
        try
        {
            foreach (var record in records)
                _history.Add(record);
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Format(Strings.History_SaveError, ex.Message);
        }
    }

    private IReadOnlyList<SessionRecord> LoadHistorySafe()
    {
        try
        {
            return _history.Load();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    private void ShowMenu() => CurrentViewModel = _menu;

    /// <summary>Asks where to save, then writes 20 tasks with the menu's settings. Returns the message to show, or null if cancelled.</summary>
    private async Task<string?> SaveWorksheetAsync(GameSettings settings)
    {
        var target = await _fileSaver.PickAsync(Strings.Menu_Worksheet,
            $"{Strings.Worksheet_Title} {DateTime.Now:yyyy-MM-dd}.pdf", "PDF", "pdf");
        if (target is null)
            return null;

        try
        {
            await using (target.Stream)
            {
                var generator = new TaskGenerator();
                var tasks = Enumerable.Range(0, WorksheetPdf.TaskCount).Select(_ => generator.Next(settings)).ToList();
                var content = new WorksheetContent($"Math Exam – {Strings.Worksheet_Title}", DescribeSettings(settings),
                    Strings.Worksheet_Name, Strings.Worksheet_Date, DateTime.Now.ToString("d"), Strings.Worksheet_Answers, tasks);
                WorksheetPdf.Write(target.Stream, content, InterFonts.Regular, InterFonts.SemiBold);
            }
            return string.Format(Strings.Menu_WorksheetSaved, target.DisplayName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Format(Strings.Menu_WorksheetError, ex.Message);
        }
    }

    /// <summary>Asks where to save, then writes the history as CSV. Returns the message to show, or null if cancelled.</summary>
    private async Task<string?> ExportHistoryAsync(IReadOnlyList<SessionRecord> records)
    {
        var target = await _fileSaver.PickAsync(Strings.History_Export, $"MathExam {DateTime.Now:yyyy-MM-dd}.csv", "CSV", "csv");
        if (target is null)
            return null;

        try
        {
            await using (var writer = new StreamWriter(target.Stream, HistoryCsv.Encoding))
                HistoryCsv.Write(writer, records);
            return string.Format(Strings.History_Exported, target.DisplayName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Format(Strings.History_ExportError, ex.Message);
        }
    }

    /// <summary>"Numbers 1 – 10, + − × ÷", plus the kind of numbers and the missing operator when chosen.</summary>
    private static string DescribeSettings(GameSettings settings)
    {
        var parts = new List<string>
        {
            string.Format(Strings.Worksheet_Settings, settings.Min, settings.Max,
                string.Join(" ", settings.Operations.Order().Select(o => o.Symbol()))),
        };
        if (settings.Numbers != NumberKind.Whole)
            parts.Add(NumberOption.Find(settings.Numbers).DisplayName);
        if (settings.MissingOperator)
            parts.Add(Strings.Menu_MissingOperator);
        return string.Join(" · ", parts);
    }
}
