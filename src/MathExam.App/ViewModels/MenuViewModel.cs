using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly Action<GameSettings, bool> _onStart;
    private readonly Action _onShowHistory;

    /// <param name="onStart">Called with the settings and whether adaptive difficulty is on.</param>
    public MenuViewModel(Action<GameSettings, bool> onStart, Action onShowHistory)
    {
        _onStart = onStart;
        _onShowHistory = onShowHistory;
    }

    [ObservableProperty]
    private bool _useAdaptive = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string _minText = "1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string _maxText = "10";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _useAdd = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _useSubtract = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _useMultiply = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _useDivide = true;

    public string? ErrorMessage => TryBuildSettings(out _);

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (TryBuildSettings(out var settings) is null)
            _onStart(settings!, UseAdaptive);
    }

    private bool CanStart() => ErrorMessage is null;

    [RelayCommand]
    private void ShowHistory() => _onShowHistory();

    /// <summary>Returns an error message, or null and the settings when the input is valid.</summary>
    private string? TryBuildSettings(out GameSettings? settings)
    {
        settings = null;
        if (!int.TryParse(MinText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var min))
            return "The lowest number must be a whole number.";
        if (!int.TryParse(MaxText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var max))
            return "The highest number must be a whole number.";

        var ops = new List<Operation>();
        if (UseAdd) ops.Add(Operation.Add);
        if (UseSubtract) ops.Add(Operation.Subtract);
        if (UseMultiply) ops.Add(Operation.Multiply);
        if (UseDivide) ops.Add(Operation.Divide);

        var candidate = new GameSettings(min, max, ops);
        var error = candidate.Validate();
        if (error is null)
            settings = candidate;
        return error;
    }
}
