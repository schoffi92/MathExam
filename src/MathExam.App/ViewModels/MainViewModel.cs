using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Navigates between the menu, game, summary, history and family screens.</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly MenuViewModel _menu;
    private readonly FamilySetupViewModel _familySetup;
    private readonly HistoryStore _history = new(HistoryStore.DefaultPath);

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel()
    {
        Display = new DisplayViewModel(new PreferencesStore(PreferencesStore.DefaultPath));
        // The menu and family setup are reused so settings and player names survive a game.
        _menu = new MenuViewModel(Display, StartGame, ShowFamilySetup, ShowHistory);
        _familySetup = new FamilySetupViewModel(StartFamilyGame, ShowMenu);
        _currentViewModel = _menu;
    }

    public DisplayViewModel Display { get; }

    private void StartGame(GameSettings settings, bool adaptive)
    {
        DifficultyAdjuster? difficulty = null;
        if (adaptive)
            difficulty = new DifficultyAdjuster(HistoryStore.ResumeLevel(LoadHistorySafe(), settings));

        CurrentViewModel = new GameViewModel(new GameSession(settings, difficulty: difficulty), ShowSummary);
    }

    private void ShowSummary(GameSession session)
    {
        var saveError = session.AnsweredCount > 0 ? SaveToHistory([SessionRecord.FromSession(session)]) : null;
        CurrentViewModel = new SummaryViewModel(session, saveError, ShowMenu);
    }

    private void ShowFamilySetup(GameSettings settings)
    {
        _familySetup.Open(settings);
        CurrentViewModel = _familySetup;
    }

    /// <summary>Family games are always adaptive: every player resumes at their own last level.</summary>
    private void StartFamilyGame(GameSettings settings, IReadOnlyList<string> names)
    {
        var history = LoadHistorySafe();
        var players = names.Select(name => (name, HistoryStore.ResumeLevel(history, settings, name)));
        CurrentViewModel = new FamilyGameViewModel(new FamilyGame(settings, players), game => ShowFamilySummary(game, names));
    }

    private void ShowFamilySummary(FamilyGame game, IReadOnlyList<string> names)
    {
        var records = game.Players
            .Where(p => p.Session.AnsweredCount > 0)
            .Select(p => SessionRecord.FromSession(p.Session, p.Name))
            .ToList();
        var saveError = records.Count > 0 ? SaveToHistory(records) : null;
        CurrentViewModel = new FamilySummaryViewModel(game, saveError, () => StartFamilyGame(game.Settings, names), ShowMenu);
    }

    private void ShowHistory()
    {
        try
        {
            CurrentViewModel = new HistoryViewModel(_history.Load(), null, ShowMenu);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            CurrentViewModel = new HistoryViewModel([], $"The history could not be read: {ex.Message}", ShowMenu);
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
            return $"The game could not be saved to the history: {ex.Message}";
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
}
