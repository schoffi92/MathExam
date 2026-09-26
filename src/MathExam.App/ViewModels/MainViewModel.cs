using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Navigates between the screens, and owns the history, the profiles and the chosen player.</summary>
public partial class MainViewModel : ObservableObject
{
    /// <summary>How long a timed challenge lasts.</summary>
    public static readonly TimeSpan ChallengeTime = TimeSpan.FromMinutes(1);

    private readonly IFileSaver _fileSaver;
    private readonly MenuViewModel _menu;
    private readonly FamilySetupViewModel _familySetup;
    private readonly HistoryStore _history = new(HistoryStore.DefaultPath);
    private readonly ProfileStore _profileStore = new(ProfileStore.DefaultPath);
    private ProfileList _profiles;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(IFileSaver fileSaver)
    {
        _fileSaver = fileSaver;
        Display = new DisplayViewModel(new PreferencesStore(PreferencesStore.DefaultPath));
        _profiles = _profileStore.Load();
        // The menu and family setup are reused so settings and player names survive a game.
        _menu = new MenuViewModel(Display, StartGame, ShowFamilySetup, ShowHistory, ShowWorksheet, ShowPlayers, SelectPlayer,
            () => CurrentViewModel = new HelpViewModel(ShowMenu));
        _familySetup = new FamilySetupViewModel(StartFamilyGame, ShowMenu);
        _currentViewModel = _menu;
        RefreshMenu();
    }

    public DisplayViewModel Display { get; }

    /// <summary>The profile that plays solo games; null for the guest.</summary>
    private string? CurrentProfile => _profiles.Find(_profiles.Current)?.Name;

    private void StartGame(GameSettings settings, bool adaptive, GameModeOption mode)
    {
        var history = LoadHistorySafe();
        // A test measures the whole range, so it does not use the adaptive levels.
        DifficultyAdjuster? difficulty = null;
        if (adaptive && mode.Mode != GameMode.Test)
            difficulty = new DifficultyAdjuster(HistoryStore.ResumeLevel(history, settings, profile: CurrentProfile));

        var session = new GameSession(settings, difficulty: difficulty,
            timeLimit: mode.Mode == GameMode.Timed ? ChallengeTime : null,
            taskLimit: mode.Mode == GameMode.Test ? mode.TaskCount : null);
        CurrentViewModel = new GameViewModel(session, ShowSummary, CurrentProfile, GoalProgress(history, CurrentProfile));
    }

    private void ShowSummary(GameSession session)
    {
        var record = SessionRecord.FromSession(session, profile: CurrentProfile);
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
            CurrentViewModel = new HistoryViewModel(_history.Load(), _profiles, null, ShowMenu, ExportHistoryAsync);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            CurrentViewModel = new HistoryViewModel([], _profiles, string.Format(Strings.History_LoadError, ex.Message), ShowMenu,
                ExportHistoryAsync);
        }
    }

    private void ShowPlayers() => CurrentViewModel = new PlayersViewModel(_profiles, profiles =>
    {
        SaveProfiles(profiles);
        ShowMenu();
    });

    private void SelectPlayer(string? name)
    {
        if (string.Equals(name, _profiles.Current, StringComparison.OrdinalIgnoreCase))
            return;
        SaveProfiles(_profiles with { Current = name });
        RefreshMenu();
    }

    private void SaveProfiles(ProfileList profiles)
    {
        _profiles = profiles;
        try
        {
            _profileStore.Save(profiles);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Not worth interrupting the player: the change still applies for this session.
        }
    }

    private void ShowWorksheet(GameSettings settings) =>
        CurrentViewModel = new WorksheetViewModel(settings, SaveWorksheetAsync, ShowMenu);

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

    private void ShowMenu()
    {
        RefreshMenu();
        CurrentViewModel = _menu;
    }

    /// <summary>Gives the menu the current players and the chosen player's progress towards their goal.</summary>
    private void RefreshMenu()
    {
        _menu.SetPlayers(_profiles.Profiles.Select(p => p.Name).ToList(), CurrentProfile);
        _menu.GoalText = GoalProgress(LoadHistorySafe(), CurrentProfile).Text(0);
    }

    private DailyGoalProgress GoalProgress(IReadOnlyList<SessionRecord> history, string? profile) =>
        _profiles.Find(profile) is { DailyGoal: > 0 } p
            ? new DailyGoalProgress(PlayerStats.TasksOn(history, p.Name, DateOnly.FromDateTime(DateTime.Now)), p.DailyGoal)
            : DailyGoalProgress.None;

    /// <summary>
    /// Asks where to save, then writes the worksheet: every version's tasks, then every answer key. The same code
    /// (and settings) always gives the same tasks; without a code a new one is picked and printed on the sheet.
    /// Returns the message to show, or null if cancelled.
    /// </summary>
    private async Task<string?> SaveWorksheetAsync(GameSettings settings, int taskCount, int versions, int? code)
    {
        var target = await _fileSaver.PickAsync(Strings.Menu_Worksheet,
            $"{Strings.Worksheet_Title} {DateTime.Now:yyyy-MM-dd}.pdf", "PDF", "pdf");
        if (target is null)
            return null;

        var sheetCode = code ?? Random.Shared.Next(1000, 10000);
        try
        {
            await using (target.Stream)
            {
                var sheets = Enumerable.Range(0, versions).Select(v =>
                {
                    var generator = new TaskGenerator(new Random(sheetCode * WorksheetViewModel.MaxVersions + v));
                    var tasks = Enumerable.Range(0, taskCount).Select(_ => generator.Next(settings)).ToList();
                    var label = $"{Strings.Worksheet_Code} {sheetCode}";
                    if (versions > 1)
                        label = $"{string.Format(Strings.Worksheet_Version, (char)('A' + v))} · {label}";
                    return new WorksheetSheet(label, tasks);
                }).ToList();
                var content = new WorksheetContent($"Math Exam – {Strings.Worksheet_Title}", DescribeSettings(settings),
                    Strings.Worksheet_Name, Strings.Worksheet_Date, DateTime.Now.ToString("d"), Strings.Worksheet_Answers, sheets);
                WorksheetPdf.Write(target.Stream, content, InterFonts.Regular, InterFonts.SemiBold);
            }
            return string.Format(Strings.Menu_WorksheetSaved, target.DisplayName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Format(Strings.Menu_WorksheetError, ex.Message);
        }
    }

    /// <summary>Asks where to save, then writes the records as CSV. Returns the message to show, or null if cancelled.</summary>
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

/// <summary>Tasks answered today towards a daily goal (a goal of 0 means none).</summary>
public sealed record DailyGoalProgress(int TasksBefore, int Goal)
{
    public static DailyGoalProgress None { get; } = new(0, 0);

    /// <summary>"Today: 12 / 20 tasks", "✓ Daily goal reached!", or empty without a goal.</summary>
    /// <param name="answeredNow">Tasks answered in the game being played, on top of the earlier ones.</param>
    public string Text(int answeredNow)
    {
        if (Goal == 0)
            return "";
        var done = TasksBefore + answeredNow;
        return done >= Goal ? Strings.Goal_Done : string.Format(Strings.Goal_Progress, done, Goal);
    }
}
