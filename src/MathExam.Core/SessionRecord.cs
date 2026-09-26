using System.Text.Json.Serialization;

namespace MathExam.Core;

/// <summary>How a game was played.</summary>
public enum GameMode
{
    Practice,
    Timed,
    Test,
    Family,
}

/// <summary>A finished game as stored in the progress history.</summary>
/// <param name="Player">The player's name in a family game; null for a solo game.</param>
/// <param name="ByOperation">Right and wrong answers per operation; null in games saved before it was recorded.</param>
/// <param name="TimeLimitSeconds">The time limit of a timed challenge; null for an untimed game.</param>
/// <param name="Profile">The profile that played a solo game; null for the guest and in family games.</param>
/// <param name="TaskLimit">The number of tasks in a test; null for other games.</param>
public sealed record SessionRecord(
    DateTime StartedAt,
    TimeSpan Duration,
    int Min,
    int Max,
    Operation[] Operations,
    int Correct,
    int Wrong,
    int? StartLevel,
    int? EndLevel,
    string? Player = null,
    IReadOnlyDictionary<Operation, OperationStats>? ByOperation = null,
    int? TimeLimitSeconds = null,
    NumberKind Numbers = NumberKind.Whole,
    bool MissingOperator = false,
    string? Profile = null,
    int? TaskLimit = null)
{
    /// <summary>Who played: the family player or the solo game's profile; null for the guest.</summary>
    [JsonIgnore]
    public string? Who => Player ?? Profile;

    [JsonIgnore]
    public GameMode Mode => Player is not null ? GameMode.Family
        : TaskLimit is not null ? GameMode.Test
        : TimeLimitSeconds is not null ? GameMode.Timed
        : GameMode.Practice;

    [JsonIgnore]
    public int Answered => Correct + Wrong;

    [JsonIgnore]
    public double Accuracy => Answered == 0 ? 0 : (double)Correct / Answered;

    [JsonIgnore]
    public bool IsAdaptive => EndLevel is not null;

    /// <summary>Whether another record was played with the same range, operations and options.</summary>
    public bool HasSameSettings(SessionRecord other) =>
        Min == other.Min && Max == other.Max && Numbers == other.Numbers && MissingOperator == other.MissingOperator
        && Operations.Order().SequenceEqual(other.Operations.Order());

    /// <param name="player">The family player's name; null for a solo game.</param>
    /// <param name="profile">The profile of a solo game; null for the guest.</param>
    public static SessionRecord FromSession(GameSession session, string? player = null, string? profile = null) => new(
        session.StartedAt,
        session.Elapsed,
        session.Settings.Min,
        session.Settings.Max,
        session.Settings.Operations.OrderBy(o => o).ToArray(),
        session.CorrectCount,
        session.WrongCount,
        session.StartLevel,
        session.Level,
        player,
        session.ByOperation.ToDictionary(),
        (int?)session.TimeLimit?.TotalSeconds,
        session.Settings.Numbers,
        session.Settings.MissingOperator,
        profile,
        session.TaskLimit);
}
