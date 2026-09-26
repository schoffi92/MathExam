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

public partial class MenuViewModel : ObservableObject
{
    private readonly Action<GameSettings, bool, bool> _onStart;
    private readonly Action<GameSettings> _onFamily;
    private readonly Action _onShowHistory;
    private readonly Func<GameSettings, Task<string?>> _onWorksheet;

    /// <param name="onStart">Called with the settings, whether adaptive difficulty is on, and whether the game is timed.</param>
    /// <param name="onFamily">Called with the settings to set up a family game.</param>
    /// <param name="onWorksheet">Saves a worksheet with the settings; returns a message to show, or null if cancelled.</param>
    public MenuViewModel(DisplayViewModel display, Action<GameSettings, bool, bool> onStart, Action<GameSettings> onFamily,
        Action onShowHistory, Func<GameSettings, Task<string?>> onWorksheet)
    {
        Display = display;
        _onStart = onStart;
        _onFamily = onFamily;
        _onShowHistory = onShowHistory;
        _onWorksheet = onWorksheet;
    }

    public DisplayViewModel Display { get; }

    public IReadOnlyList<NumberOption> NumberOptions => NumberOption.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FamilyCommand), nameof(WorksheetCommand))]
    private NumberOption _numbers = NumberOption.Find(NumberKind.Whole);

    [ObservableProperty]
    private bool _useMissingOperator;

    [ObservableProperty]
    private bool _useTimed;

    /// <summary>The result of the last worksheet save.</summary>
    [ObservableProperty]
    private string _statusText = "";

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

    public string? ErrorMessage => TryBuildSettings(out _);

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (TryBuildSettings(out var settings) is null)
            _onStart(settings!, UseAdaptive, UseTimed);
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
    private async Task Worksheet()
    {
        if (TryBuildSettings(out var settings) is null && await _onWorksheet(settings!) is { } message)
            StatusText = message;
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

        var candidate = new GameSettings(min, max, ops, Numbers.Kind, UseMissingOperator);
        var error = candidate.Validate();
        if (error is null)
            settings = candidate;
        return error;
    }
}
