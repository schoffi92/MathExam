using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;

namespace MathExam.App;

/// <summary>Switches between the standard (Light) and high-contrast theme variants.</summary>
public static class ThemeManager
{
    /// <summary>
    /// Custom theme variant with its own resource dictionary (Themes/HighContrast.axaml). It inherits
    /// Dark, so Fluent controls we do not restyle (check boxes, data grid, scroll bars) render light-on-dark.
    /// </summary>
    public static readonly ThemeVariant HighContrast = new("HighContrast", ThemeVariant.Dark);

    public static void Apply(bool highContrast)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = highContrast ? HighContrast : ThemeVariant.Light;
    }

    /// <summary>True when the operating system's high-contrast / increased-contrast mode is on.</summary>
    public static bool SystemPrefersHighContrast() =>
        Application.Current?.PlatformSettings?.GetColorValues().ContrastPreference == ColorContrastPreference.High;
}
