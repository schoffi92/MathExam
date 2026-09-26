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
    /// The level to resume at: the end level of the latest adaptive game with the same range and kind of numbers,
    /// played by the same family player, or solo (<paramref name="player"/> null) under the same profile
    /// (null for the guest). Names are compared case-insensitively.
    /// </summary>
    public static int ResumeLevel(IEnumerable<SessionRecord> records, GameSettings settings, string? player = null,
        string? profile = null) =>
        records.LastOrDefault(r => r.EndLevel is not null && r.Min == settings.Min && r.Max == settings.Max
                                   && r.Numbers == settings.Numbers
                                   && string.Equals(r.Player, player, StringComparison.OrdinalIgnoreCase)
                                   && (player is not null || string.Equals(r.Profile, profile, StringComparison.OrdinalIgnoreCase)))
            ?.EndLevel
        ?? DifficultyAdjuster.MinLevel;

    /// <summary>
    /// The most correct answers in an earlier timed solo game by the same profile, with the same time limit and
    /// settings as <paramref name="game"/>, or null when there is none (or <paramref name="game"/> is not timed).
    /// </summary>
    public static int? PersonalBest(IEnumerable<SessionRecord> records, SessionRecord game) =>
        game.TimeLimitSeconds is null
            ? null
            : records
                .Where(r => r.Player is null && string.Equals(r.Profile, game.Profile, StringComparison.OrdinalIgnoreCase)
                            && r.TimeLimitSeconds == game.TimeLimitSeconds && r.HasSameSettings(game))
                .Max(r => (int?)r.Correct);
}
