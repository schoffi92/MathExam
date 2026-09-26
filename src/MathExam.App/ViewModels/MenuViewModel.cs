using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>A choice in the "Numbers" list.</summary>
public sealed record NumberOption(NumberKind Kind)
{
    public static IReadOnlyList<NumberOption> All { get; } = Enum.GetValues<NumberKind>().Select(k => new NumberOption(k)).ToList();

    public static NumberOption Find(NumberKind kind) => All.First(o => o.Kind == kind);

    // Read when shown, so it follows the current language.
    public string DisplayName => Kind switch
    {
        NumberKind.Decimal => Strings.Menu_NumbersDecimal,
        NumberKind.Fraction => Strings.Menu_NumbersFraction,
        _ => Strings.Menu_NumbersWhole,
    };
}

/// <summary>A choice in the "Game" list: practice, a timed challenge, or a test of a number of tasks.</summary>
public sealed record GameModeOption(GameMode Mode, int TaskCount = 0)
{
    public static IReadOnlyList<GameModeOption> All { get; } =
        [new(GameMode.Practice), new(GameMode.Timed), new(GameMode.Test, 10), new(GameMode.Test, 20)];

    public string DisplayName => Mode switch
    {
        GameMode.Timed => Strings.Mode_Timed,
        GameMode.Test => string.Format(Strings.Mode_Test, TaskCount),
        _ => Strings.Mode_Practice,
    };
}

/// <summary>A choice in the "Player" list; <see cref="Name"/> is null for the guest.</summary>
public sealed record PlayerOption(string? Name)
{
    public string DisplayName => Name ?? Strings.Menu_Guest;
}

public partial class MenuViewModel : ObservableObject
{
    private readonly Action<GameSettings, bool, GameModeOption> _onStart;
    private readonly Action<GameSettings> _onFamily;
    private readonly Action _onShowHistory;
    private readonly Action<GameSettings> _onWorksheet;
    private readonly Action _onPlayers;
    private readonly Action<string?> _onSelectPlayer;
    private bool _settingPlayers;

    /// <param name="onStart">Called with the settings, whether adaptive difficulty is on, and the game mode.</param>
    /// <param name="onFamily">Called with the settings to set up a family game.</param>
    /// <param name="onWorksheet">Opens the worksheet options with the settings.</param>
    /// <param name="onPlayers">Opens the screen for adding and removing players.</param>
    /// <param name="onSelectPlayer">Called with the chosen player's name (null for the guest).</param>
    public MenuViewModel(DisplayViewModel display, Action<GameSettings, bool, GameModeOption> onStart, Action<GameSettings> onFamily,
        Action onShowHistory, Action<GameSettings> onWorksheet, Action onPlayers, Action<string?> onSelectPlayer)
    {
        Display = display;
        _onStart = onStart;
        _onFamily = onFamily;
        _onShowHistory = onShowHistory;
        _onWorksheet = onWorksheet;
        _onPlayers = onPlayers;
        _onSelectPlayer = onSelectPlayer;
    }

    public DisplayViewModel Display { get; }

    public IReadOnlyList<GameModeOption> Modes => GameModeOption.All;

    [ObservableProperty]
    private GameModeOption _mode = GameModeOption.All[0];

    /// <summary>The guest, then each profile.</summary>
    [ObservableProperty]
    private IReadOnlyList<PlayerOption> _players = [new PlayerOption(null)];

    [ObservableProperty]
    private PlayerOption? _selectedPlayer;

    /// <summary>The chosen player's progress towards their daily goal, or empty.</summary>
    [ObservableProperty]
    private string _goalText = "";

    /// <summary>Refills the player list, e.g. after the Players screen, and selects <paramref name="current"/>.</summary>
    public void SetPlayers(IReadOnlyList<string> names, string? current)
    {
        _settingPlayers = true;
        Players = [new PlayerOption(null), .. names.Select(n => new PlayerOption(n))];
        SelectedPlayer = Players.FirstOrDefault(p => string.Equals(p.Name, current, StringComparison.OrdinalIgnoreCase)) ?? Players[0];
        _settingPlayers = false;
    }

    partial void OnSelectedPlayerChanged(PlayerOption? value)
    {
        if (!_settingPlayers && value is not null)
            _onSelectPlayer(value.Name);
    }

    [RelayCommand]
    private void ManagePlayers() => _onPlayers();

    public IReadOnlyList<NumberOption> NumberOptions => NumberOption.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private NumberOption _numbers = NumberOption.Find(NumberKind.Whole);

    [ObservableProperty]
    private bool _useMissingOperator;

    [ObservableProperty]
    private bool _useAdaptive = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private string _minText = "1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private string _maxText = "10";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useAdd = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useSubtract = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useMultiply = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useDivide = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _usePower;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useRoot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private bool _useConvert;

    public string? ErrorMessage => TryBuildSettings(out _);

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (TryBuildSettings(out var settings) is null)
            _onStart(settings!, UseAdaptive, Mode);
    }

    private bool CanStart() => ErrorMessage is null;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Family()
    {
        if (TryBuildSettings(out var settings) is null)
            _onFamily(settings!);
    }

    [RelayCommand]
    private void ShowHistory() => _onShowHistory();

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Worksheet()
    {
        if (TryBuildSettings(out var settings) is null)
            _onWorksheet(settings!);
    }

    /// <summary>Returns an error message, or null and the settings when the input is valid.</summary>
    private string? TryBuildSettings(out GameSettings? settings)
    {
        settings = null;
        if (!int.TryParse(MinText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var min))
            return Strings.Menu_MinNotWhole;
        if (!int.TryParse(MaxText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var max))
            return Strings.Menu_MaxNotWhole;

        var ops = new List<Operation>();
        if (UseAdd) ops.Add(Operation.Add);
        if (UseSubtract) ops.Add(Operation.Subtract);
        if (UseMultiply) ops.Add(Operation.Multiply);
        if (UseDivide) ops.Add(Operation.Divide);
        if (UsePower) ops.Add(Operation.Power);
        if (UseRoot) ops.Add(Operation.Root);
        if (UseConvert) ops.Add(Operation.Convert);

        var candidate = new GameSettings(min, max, ops, Numbers.Kind, UseMissingOperator);
        var error = candidate.Validate();
        if (error is null)
            settings = candidate;
        return error;
    }
}
