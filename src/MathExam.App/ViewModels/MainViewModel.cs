using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Navigates between the menu, game and summary screens.</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly MenuViewModel _menu;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel()
    {
        // The menu is reused so the player's last settings survive a game.
        _menu = new MenuViewModel(StartGame);
        _currentViewModel = _menu;
    }

    private void StartGame(GameSettings settings) =>
        CurrentViewModel = new GameViewModel(new GameSession(settings), ShowSummary);

    private void ShowSummary(GameSession session) =>
        CurrentViewModel = new SummaryViewModel(session, ShowMenu);

    private void ShowMenu() => CurrentViewModel = _menu;
}
