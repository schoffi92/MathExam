using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Navigates between the menu, game, summary and history screens.</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly MenuViewModel _menu;
    private readonly HistoryStore _history = new(HistoryStore.DefaultPath);

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel()
    {
        // The menu is reused so the player's last settings survive a game.
        _menu = new MenuViewModel(StartGame, ShowHistory);
        _currentViewModel = _menu;
    }

    private void StartGame(GameSettings settings, bool adaptive)
    {
        DifficultyAdjuster? difficulty = null;
        if (adaptive)
            difficulty = new DifficultyAdjuster(HistoryStore.ResumeLevel(LoadHistorySafe(), settings));

        CurrentViewModel = new GameViewModel(new GameSession(settings, difficulty: difficulty), ShowSummary);
    }

    private void ShowSummary(GameSession session)
    {
        string? saveError = null;
        if (session.AnsweredCount > 0)
        {
            try
            {
                _history.Add(SessionRecord.FromSession(session));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                saveError = $"The game could not be saved to the history: {ex.Message}";
            }
        }

        CurrentViewModel = new SummaryViewModel(session, saveError, ShowMenu);
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
