using CommunityToolkit.Mvvm.ComponentModel;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>Text size, colour theme and language. Changes apply immediately and are saved.</summary>
public partial class DisplayViewModel : ObservableObject
{
    private readonly PreferencesStore _store;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextScale), nameof(IsNormalText), nameof(IsLargeText), nameof(IsExtraLargeText))]
    private TextSize _textSize;

    [ObservableProperty]
    private bool _highContrast;

    [ObservableProperty]
    private LanguageOption _language;

    public DisplayViewModel(PreferencesStore store)
    {
        _store = store;
        // First launch: follow the operating system's high-contrast setting.
        var saved = store.Load() ?? new DisplayPreferences(HighContrast: ThemeManager.SystemPrefersHighContrast());
        _textSize = saved.TextSize;
        _highContrast = saved.HighContrast;
        _language = LanguageManager.Find(saved.Language);
        ThemeManager.Apply(_highContrast);
        LanguageManager.Apply(_language.Code);
    }

    public IReadOnlyList<LanguageOption> Languages => LanguageManager.Options;

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

    // MainWindow listens for this change and rebuilds the screen, so every text is read again.
    partial void OnLanguageChanged(LanguageOption value)
    {
        LanguageManager.Apply(value.Code);
        Save();
    }

    private void Save()
    {
        try
        {
            _store.Save(new DisplayPreferences(TextSize, HighContrast, Language.Code));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Not worth interrupting the player: the change still applies for this session.
        }
    }
}
