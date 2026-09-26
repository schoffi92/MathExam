using System.Text.Json;
using MathExam.Core.Resources;

namespace MathExam.Core;

/// <summary>A child (or pupil) who plays under their own name.</summary>
/// <param name="DailyGoal">Tasks to answer per day; 0 means no goal.</param>
public sealed record Profile(string Name, int DailyGoal = 0);

/// <summary>The saved profiles and the one that played last (null for the guest).</summary>
public sealed record ProfileList(IReadOnlyList<Profile> Profiles, string? Current = null)
{
    public const int MaxDailyGoal = 500;

    public static ProfileList Empty { get; } = new([]);

    public Profile? Find(string? name) =>
        name is null ? null : Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns a user-facing error for a new profile name, or null when it can be added.</summary>
    public string? ValidateNewName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length is 0 or > FamilyGame.MaxNameLength)
            return trimmed.Length == 0 ? Strings.Players_EmptyName : string.Format(Strings.Players_NameTooLong, FamilyGame.MaxNameLength);
        return Find(trimmed) is null ? null : Strings.Profiles_NameTaken;
    }
}

/// <summary>Persists the <see cref="ProfileList"/> as a small JSON file.</summary>
public sealed class ProfileStore
{
    public ProfileStore(string filePath)
    {
        FilePath = filePath;
    }

    public static string DefaultPath => Path.Combine(JsonFile.AppDataDirectory, "profiles.json");

    public string FilePath { get; }

    /// <summary>The saved profiles; empty when none are saved or the file is unreadable.</summary>
    public ProfileList Load()
    {
        if (!File.Exists(FilePath))
            return ProfileList.Empty;
        try
        {
            return JsonFile.Read<ProfileList>(FilePath) ?? ProfileList.Empty;
        }
        catch (JsonException)
        {
            return ProfileList.Empty;
        }
    }

    public void Save(ProfileList profiles) => JsonFile.WriteAtomic(FilePath, profiles);
}

/// <summary>Daily goals and a per-player overview of the history, e.g. for a teacher.</summary>
public static class PlayerStats
{
    /// <summary>
    /// Tasks answered on <paramref name="day"/> by <paramref name="name"/>: solo games under that profile
    /// plus family games played under that name (names compared case-insensitively).
    /// </summary>
    public static int TasksOn(IEnumerable<SessionRecord> records, string name, DateOnly day) =>
        records.Where(r => IsPlayedBy(r, name) && DateOnly.FromDateTime(r.StartedAt) == day).Sum(r => r.Answered);

    public static bool IsPlayedBy(SessionRecord record, string? name) =>
        string.Equals(record.Who, name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// One row per profile, plus one per other name in the history (family players, and the guest for
    /// solo games without a profile, whose name is null). Profiles come first, in their order.
    /// </summary>
    public static IReadOnlyList<PlayerOverview> Overview(IReadOnlyCollection<SessionRecord> records, ProfileList profiles, DateOnly today)
    {
        var names = profiles.Profiles.Select(p => (string?)p.Name).ToList();
        foreach (var who in records.Select(r => r.Who))
        {
            if (!names.Any(n => string.Equals(n, who, StringComparison.OrdinalIgnoreCase)))
                names.Add(who);
        }

        return names.Select(name =>
        {
            var own = records.Where(r => IsPlayedBy(r, name)).ToList();
            var totals = HistoryTotals.From(own);
            return new PlayerOverview(
                name,
                totals.Games,
                totals.Tasks,
                totals.Accuracy,
                totals.Time,
                name is null ? 0 : TasksOn(own, name, today),
                profiles.Find(name)?.DailyGoal ?? 0,
                own.Count == 0 ? null : own.Max(r => r.StartedAt));
        }).ToList();
    }
}

/// <param name="Name">The player; null for the guest (solo games without a profile).</param>
public sealed record PlayerOverview(
    string? Name, int Games, int Tasks, double Accuracy, TimeSpan Time, int TasksToday, int DailyGoal, DateTime? LastPlayed);
