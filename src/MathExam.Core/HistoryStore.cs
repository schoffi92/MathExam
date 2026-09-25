using System.Text.Json;

namespace MathExam.Core;

/// <summary>Persists finished games as a JSON array in a single file.</summary>
public sealed class HistoryStore
{
    public HistoryStore(string filePath)
    {
        FilePath = filePath;
    }

    public static string DefaultPath => Path.Combine(JsonFile.AppDataDirectory, "history.json");

    public string FilePath { get; }

    /// <summary>
    /// Returns all records, oldest first. A missing file means no history; an unreadable file is
    /// copied to "*.bak" so the next save does not destroy it, and an empty history is returned.
    /// </summary>
    public IReadOnlyList<SessionRecord> Load()
    {
        if (!File.Exists(FilePath))
            return [];

        try
        {
            return JsonFile.Read<List<SessionRecord>>(FilePath) ?? [];
        }
        catch (JsonException)
        {
            File.Copy(FilePath, FilePath + ".bak", overwrite: true);
            return [];
        }
    }

    public void Add(SessionRecord record) => JsonFile.WriteAtomic(FilePath, Load().Append(record).ToList());

    /// <summary>
    /// The level to resume at: the end level of the latest adaptive game with the same range, played solo
    /// (<paramref name="player"/> null) or by the same family player (name compared case-insensitively).
    /// </summary>
    public static int ResumeLevel(IEnumerable<SessionRecord> records, GameSettings settings, string? player = null) =>
        records.LastOrDefault(r => r.EndLevel is not null && r.Min == settings.Min && r.Max == settings.Max
                                   && string.Equals(r.Player, player, StringComparison.OrdinalIgnoreCase))?.EndLevel
        ?? DifficultyAdjuster.MinLevel;
}
