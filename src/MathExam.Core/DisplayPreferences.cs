using System.Text.Json;

namespace MathExam.Core;

public enum TextSize
{
    Normal,
    Large,
    ExtraLarge,
}

/// <summary>How the app looks: text size and colour theme.</summary>
public sealed record DisplayPreferences(TextSize TextSize = TextSize.Normal, bool HighContrast = false);

/// <summary>Persists <see cref="DisplayPreferences"/> as a small JSON file.</summary>
public sealed class PreferencesStore
{
    public PreferencesStore(string filePath)
    {
        FilePath = filePath;
    }

    public static string DefaultPath => Path.Combine(JsonFile.AppDataDirectory, "preferences.json");

    public string FilePath { get; }

    /// <summary>Returns the saved preferences, or null when none are saved or the file is unreadable.</summary>
    public DisplayPreferences? Load()
    {
        if (!File.Exists(FilePath))
            return null;

        try
        {
            return JsonFile.Read<DisplayPreferences>(FilePath);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Save(DisplayPreferences preferences) => JsonFile.WriteAtomic(FilePath, preferences);
}
