using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One name box on the family setup screen.</summary>
public partial class PlayerNameEntry : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _label = "";

    [ObservableProperty]
    private bool _canRemove;

    public PlayerNameEntry(Action<PlayerNameEntry> onRemove)
    {
        RemoveCommand = new RelayCommand(() => onRemove(this));
    }

    public IRelayCommand RemoveCommand { get; }
}

/// <summary>Enter the players' names for a family game. Reused, so the names are kept for the next game.</summary>
public partial class FamilySetupViewModel : ObservableObject
{
    private readonly Action<GameSettings, IReadOnlyList<string>> _onStart;
    private readonly Action _onBack;
    private GameSettings? _settings;

    public FamilySetupViewModel(Action<GameSettings, IReadOnlyList<string>> onStart, Action onBack)
    {
        _onStart = onStart;
        _onBack = onBack;
        for (var i = 0; i < FamilyGame.MinPlayers; i++)
            AddEntry();
    }

    public ObservableCollection<PlayerNameEntry> Players { get; } = [];

    [ObservableProperty]
    private string _settingsText = "";

    public string? ErrorMessage => FamilyGame.ValidatePlayers(Players.Select(p => p.Name));

    /// <summary>Called each time the screen opens, with the current menu settings.</summary>
    public void Open(GameSettings settings)
    {
        _settings = settings;
        SettingsText = string.Format(Strings.FamilySetup_Settings, settings.Min, settings.Max,
            string.Join(" ", settings.Operations.OrderBy(o => o).Select(o => o.Symbol())));
        // The language may have changed since the labels were made.
        PlayersChanged();
    }

    [RelayCommand(CanExecute = nameof(CanAddPlayer))]
    private void AddPlayer() => AddEntry();

    private bool CanAddPlayer() => Players.Count < FamilyGame.MaxPlayers;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (_settings is not null && CanStart())
            _onStart(_settings, Players.Select(p => p.Name.Trim()).ToList());
    }

    private bool CanStart() => ErrorMessage is null;

    [RelayCommand]
    private void Back() => _onBack();

    private void AddEntry()
    {
        var entry = new PlayerNameEntry(Remove);
        entry.PropertyChanged += OnEntryChanged;
        Players.Add(entry);
        PlayersChanged();
    }

    private void Remove(PlayerNameEntry entry)
    {
        if (Players.Count <= FamilyGame.MinPlayers)
            return;
        entry.PropertyChanged -= OnEntryChanged;
        Players.Remove(entry);
        PlayersChanged();
    }

    private void OnEntryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerNameEntry.Name))
            ValidationChanged();
    }

    private void PlayersChanged()
    {
        for (var i = 0; i < Players.Count; i++)
        {
            Players[i].Label = string.Format(Strings.FamilySetup_Player, i + 1);
            Players[i].CanRemove = Players.Count > FamilyGame.MinPlayers;
        }
        AddPlayerCommand.NotifyCanExecuteChanged();
        ValidationChanged();
    }

    private void ValidationChanged()
    {
        OnPropertyChanged(nameof(ErrorMessage));
        StartCommand.NotifyCanExecuteChanged();
    }
}
