using System.Text.Json;
using System.Text.Json.Serialization;

namespace MathExam.Core;

/// <summary>Persists finished games as a JSON array in a single file.</summary>
public sealed class HistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public HistoryStore(string filePath)
    {
        FilePath = filePath;
    }

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathExam", "history.json");

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
            return JsonSerializer.Deserialize<List<SessionRecord>>(File.ReadAllText(FilePath), JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            File.Copy(FilePath, FilePath + ".bak", overwrite: true);
            return [];
        }
    }

    public void Add(SessionRecord record)
    {
        var records = Load().Append(record).ToList();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);

        // Write to a temp file first so a crash mid-write cannot corrupt the existing history.
        var tempPath = FilePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(records, JsonOptions));
        File.Move(tempPath, FilePath, overwrite: true);
    }

    /// <summary>The level to resume at: the end level of the latest adaptive game with the same range.</summary>
    public static int ResumeLevel(IEnumerable<SessionRecord> records, GameSettings settings) =>
        records.LastOrDefault(r => r.EndLevel is not null && r.Min == settings.Min && r.Max == settings.Max)?.EndLevel
        ?? DifficultyAdjuster.MinLevel;
}
