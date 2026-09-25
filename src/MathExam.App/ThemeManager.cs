using System.Windows;

namespace MathExam.App;

/// <summary>Switches the colour theme by replacing the theme dictionary merged into App.xaml.</summary>
public static class ThemeManager
{
    private static readonly Uri StandardTheme = new("pack://application:,,,/Themes/Standard.xaml");
    private static readonly Uri HighContrastTheme = new("pack://application:,,,/Themes/HighContrast.xaml");

    public static void Apply(bool highContrast)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries.Clear();
        dictionaries.Add(new ResourceDictionary { Source = highContrast ? HighContrastTheme : StandardTheme });
    }
}
