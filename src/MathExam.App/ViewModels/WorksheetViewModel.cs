using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Options for a printable worksheet: how many tasks, how many versions, and an optional sheet code.</summary>
public partial class WorksheetViewModel : ObservableObject
{
    public const int MaxVersions = 4;
    public const int MaxCode = 999_999;

    private readonly GameSettings _settings;
    private readonly Func<GameSettings, int, int, int?, Task<string?>> _onSave;
    private readonly Action _onBack;

    /// <param name="onSave">Saves the PDF (settings, tasks per sheet, versions, code or null); returns a message, or null if cancelled.</param>
    public WorksheetViewModel(GameSettings settings, Func<GameSettings, int, int, int?, Task<string?>> onSave, Action onBack)
    {
        _settings = settings;
        _onSave = onSave;
        _onBack = onBack;
    }

    public IReadOnlyList<int> TaskCounts { get; } = [10, 20, 30];

    [ObservableProperty]
    private int _taskCount = 20;

    // decimal?, because that is what NumericUpDown binds to.
    [ObservableProperty]
    private decimal? _versions = 1;

    /// <summary>Empty (null) for new tasks each time.</summary>
    [ObservableProperty]
    private decimal? _code;

    public int VersionLimit => MaxVersions;
    public int CodeLimit => MaxCode;

    /// <summary>The result of the last save.</summary>
    [ObservableProperty]
    private string _statusText = "";

    [RelayCommand]
    private async Task Save()
    {
        var versions = (int)Math.Clamp(Versions ?? 1, 1, MaxVersions);
        int? code = Code is { } c ? (int)Math.Clamp(c, 1, MaxCode) : null;
        if (await _onSave(_settings, TaskCount, versions, code) is { } message)
            StatusText = message;
    }

    [RelayCommand]
    private void Back() => _onBack();
}
