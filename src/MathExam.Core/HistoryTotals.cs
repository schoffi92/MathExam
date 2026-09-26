namespace MathExam.Core;

/// <summary>
/// Totals over the whole history. A family game stores one record per player (sharing a start time);
/// it counts as one game, and its time is counted once. <see cref="ByOperation"/> covers only the games
/// that recorded it.
/// </summary>
public sealed record HistoryTotals(
    int Games, int Tasks, int Correct, TimeSpan Time, IReadOnlyDictionary<Operation, OperationStats> ByOperation)
{
    public double Accuracy => Tasks == 0 ? 0 : (double)Correct / Tasks;

    public static HistoryTotals From(IReadOnlyCollection<SessionRecord> records)
    {
        var gameDurations = records
            .Where(r => r.Player is null)
            .Select(r => r.Duration)
            .Concat(records
                .Where(r => r.Player is not null)
                .GroupBy(r => r.StartedAt)
                .Select(game => game.Max(r => r.Duration)))
            .ToList();

        var byOperation = records
            .SelectMany(r => r.ByOperation ?? new Dictionary<Operation, OperationStats>())
            .GroupBy(pair => pair.Key, pair => pair.Value)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key,
                group => new OperationStats(group.Sum(s => s.Correct), group.Sum(s => s.Wrong)));

        return new HistoryTotals(
            gameDurations.Count,
            records.Sum(r => r.Answered),
            records.Sum(r => r.Correct),
            TimeSpan.FromTicks(gameDurations.Sum(d => d.Ticks)),
            byOperation);
    }
}
