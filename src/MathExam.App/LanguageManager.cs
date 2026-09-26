using System.Globalization;
using MathExam.App.Resources;

namespace MathExam.App;

/// <summary>A choice in the language list. <see cref="Code"/> is null for "follow the operating system".</summary>
public sealed record LanguageOption(string? Code, string NativeName)
{
    // Read when the list is shown, so "System default" appears in the current language.
    public string DisplayName => Code is null ? Strings.Menu_LanguageSystem : NativeName;
}

/// <summary>Switches the UI language. Texts come from Resources/Strings*.resx.</summary>
public static class LanguageManager
{
    // Captured before the first switch, so "System default" can go back to it.
    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    /// <summary>The system default first, then each language in its own name.</summary>
    public static IReadOnlyList<LanguageOption> Options { get; } =
    [
        new(null, ""),
        new("en", "English"),
        new("fr", "Français"),
        new("de", "Deutsch"),
        new("hu", "Magyar"),
        new("it", "Italiano"),
        new("es", "Español"),
        new("pl", "Polski"),
        new("cs", "Čeština"),
        new("fi", "Suomi"),
        new("pt", "Português"),
        new("sv", "Svenska"),
        new("nb", "Norsk bokmål"),
        new("hr", "Hrvatski"),
        new("sl", "Slovenščina"),
        new("sk", "Slovenčina"),
        new("tr", "Türkçe"),
        new("el", "Ελληνικά"),
        new("ro", "Română"),
        new("nl", "Nederlands"),
    ];

    /// <summary>Returns the option for a saved code; unknown codes fall back to the system default.</summary>
    public static LanguageOption Find(string? code) =>
        Options.FirstOrDefault(o => string.Equals(o.Code, code, StringComparison.OrdinalIgnoreCase)) ?? Options[0];

    /// <summary>
    /// Sets the UI language only. Numbers and dates keep following the operating system's regional settings.
    /// A system language without a translation falls back to English.
    /// </summary>
    public static void Apply(string? code)
    {
        var culture = code is null ? SystemCulture : CultureInfo.GetCultureInfo(code);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
