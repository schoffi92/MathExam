using MathExam.Core;

namespace MathExam.Core.Tests;

public class HistoryTotalsTests
{
    private static readonly DateTime Family1 = new(2026, 9, 26, 10, 0, 0);
    private static readonly DateTime Family2 = new(2026, 9, 26, 11, 0, 0);

    private static SessionRecord Rec(DateTime started, int seconds, int correct, int wrong, string? player = null) =>
        new(started, TimeSpan.FromSeconds(seconds), 1, 10, [Operation.Add], correct, wrong, null, null, player);

    [Fact]
    public void Family_game_counts_once_with_its_time_counted_once()
    {
        var records = new[]
        {
            Rec(new DateTime(2026, 9, 25, 9, 0, 0), 60, 5, 1),        // solo
            Rec(Family1, 120, 3, 0, "Anna"), Rec(Family1, 120, 1, 2, "Ben"),
            Rec(Family2, 30, 2, 0, "Anna"), Rec(Family2, 31, 0, 1, "Grandpa"), Rec(Family2, 30, 1, 0, "Ben"),
        };

        var totals = HistoryTotals.From(records);

        Assert.Equal(3, totals.Games);                            // 1 solo + 2 family games
        Assert.Equal(6 + 6 + 4, totals.Tasks);
        Assert.Equal(5 + 4 + 3, totals.Correct);
        Assert.Equal(TimeSpan.FromSeconds(60 + 120 + 31), totals.Time);
    }

    [Fact]
    public void Empty_history_has_zero_totals()
    {
        var totals = HistoryTotals.From([]);
        Assert.Equal(0, totals.Games);
        Assert.Equal(0, totals.Accuracy);
    }

    [Fact]
    public void Family_players_share_the_start_time()
    {
        var game = new FamilyGame(new GameSettings(1, 10, [Operation.Add]), [("Anna", 1), ("Ben", 1), ("Cleo", 1)]);
        Assert.Single(game.Players.Select(p => p.Session.StartedAt).Distinct());
    }
}
