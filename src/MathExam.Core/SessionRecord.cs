using System.Text.Json.Serialization;

namespace MathExam.Core;

/// <summary>A finished game as stored in the progress history.</summary>
/// <param name="Player">The player's name in a family game; null for a solo game.</param>
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
    string? Player = null)
{
    [JsonIgnore]
    public int Answered => Correct + Wrong;

    [JsonIgnore]
    public double Accuracy => Answered == 0 ? 0 : (double)Correct / Answered;

    [JsonIgnore]
    public bool IsAdaptive => EndLevel is not null;

    public static SessionRecord FromSession(GameSession session, string? player = null) => new(
        session.StartedAt,
        session.Elapsed,
        session.Settings.Min,
        session.Settings.Max,
        session.Settings.Operations.OrderBy(o => o).ToArray(),
        session.CorrectCount,
        session.WrongCount,
        session.StartLevel,
        session.Level,
        player);
}
