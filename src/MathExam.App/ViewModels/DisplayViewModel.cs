using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Text size and colour theme. Changes apply immediately and are saved.</summary>
public partial class DisplayViewModel : ObservableObject
{
    private readonly PreferencesStore _store;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextScale), nameof(IsNormalText), nameof(IsLargeText), nameof(IsExtraLargeText))]
    private TextSize _textSize;

    [ObservableProperty]
    private bool _highContrast;

    public DisplayViewModel(PreferencesStore store)
    {
        _store = store;
        // First launch: follow the Windows high-contrast setting.
        var saved = store.Load() ?? new DisplayPreferences(HighContrast: SystemParameters.HighContrast);
        _textSize = saved.TextSize;
        _highContrast = saved.HighContrast;
        ThemeManager.Apply(_highContrast);
    }

    public double TextScale => TextSize switch
    {
        TextSize.Large => 1.25,
        TextSize.ExtraLarge => 1.5,
        _ => 1.0,
    };

    // One bool per radio button; unchecked buttons are ignored because checking another one sets the size.
    public bool IsNormalText
    {
        get => TextSize == TextSize.Normal;
        set { if (value) TextSize = TextSize.Normal; }
    }

    public bool IsLargeText
    {
        get => TextSize == TextSize.Large;
        set { if (value) TextSize = TextSize.Large; }
    }

    public bool IsExtraLargeText
    {
        get => TextSize == TextSize.ExtraLarge;
        set { if (value) TextSize = TextSize.ExtraLarge; }
    }

    partial void OnTextSizeChanged(TextSize value) => Save();

    partial void OnHighContrastChanged(bool value)
    {
        ThemeManager.Apply(value);
        Save();
    }

    private void Save()
    {
        try
        {
            _store.Save(new DisplayPreferences(TextSize, HighContrast));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Not worth interrupting the player: the change still applies for this session.
        }
    }
}
