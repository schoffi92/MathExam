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
/// the screens show it.
/// </summary>
public partial class HelpViewModel : ObservableObject, IScrollsItself
{
    public const int SectionCount = 8;

    private readonly Action _onClose;

    public HelpViewModel(Action onClose)
    {
        _onClose = onClose;
        Sections = Enumerable.Range(1, SectionCount)
            .Select(i => new HelpSection(Text($"Help_S{i}_Title"), Text($"Help_S{i}_Body")))
            .ToList();
    }

    public IReadOnlyList<HelpSection> Sections { get; }

    [RelayCommand]
    private void Close() => _onClose();

    private static string Text(string key) =>
        FillLabels(Strings.ResourceManager.GetString(key, Strings.Culture) ?? key);

    public static string FillLabels(string text) => LabelPattern().Replace(text,
        m => Strings.ResourceManager.GetString(m.Groups[1].Value, Strings.Culture) ?? m.Value);

    [GeneratedRegex("«([A-Za-z0-9_]+)»")]
    private static partial Regex LabelPattern();
}
