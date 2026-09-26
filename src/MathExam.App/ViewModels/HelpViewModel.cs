using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.App.Resources;

namespace MathExam.App.ViewModels;

/// <summary>A screen that scrolls its own content, so the window must not scroll it as a whole.</summary>
public interface IScrollsItself;

/// <summary>One part of the help: a heading and its text.</summary>
public sealed record HelpSection(string Title, string Body);

/// <summary>
/// The "How to use" screen: the Help_S{n}_Title/Body texts in the current language. A «Key» in them stands for
/// another text (a button or setting name) and is filled in here, so the help names every control exactly as
/// the screens show it. The screen has its own language list (the app's language setting), and reloads the
/// texts when the language changes.
/// </summary>
public partial class HelpViewModel : ObservableObject, IScrollsItself
{
    public const int SectionCount = 8;

    private readonly Action _onClose;

    public HelpViewModel(DisplayViewModel display, Action onClose)
    {
        Display = display;
        _onClose = onClose;
        _sections = LoadSections();
        Display.PropertyChanged += OnDisplayChanged;
    }

    /// <summary>The app's display settings; the language list is bound to <see cref="DisplayViewModel.Language"/>.</summary>
    public DisplayViewModel Display { get; }

    [ObservableProperty]
    private IReadOnlyList<HelpSection> _sections;

    [RelayCommand]
    private void Close()
    {
        Display.PropertyChanged -= OnDisplayChanged;
        _onClose();
    }

    private void OnDisplayChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The new language is already applied (DisplayViewModel does that first), so the texts come out in it.
        if (e.PropertyName == nameof(DisplayViewModel.Language))
            Sections = LoadSections();
    }

    private static IReadOnlyList<HelpSection> LoadSections() => Enumerable.Range(1, SectionCount)
        .Select(i => new HelpSection(Text($"Help_S{i}_Title"), Text($"Help_S{i}_Body")))
        .ToList();

    private static string Text(string key) =>
        FillLabels(Strings.ResourceManager.GetString(key, Strings.Culture) ?? key);

    public static string FillLabels(string text) => LabelPattern().Replace(text,
        m => Strings.ResourceManager.GetString(m.Groups[1].Value, Strings.Culture) ?? m.Value);

    [GeneratedRegex("«([A-Za-z0-9_]+)»")]
    private static partial Regex LabelPattern();
}
