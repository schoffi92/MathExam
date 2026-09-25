using MathExam.Core;

namespace MathExam.Core.Tests;

public class FamilyGameTests
{
    private static readonly GameSettings Settings = new(1, 100, [Operation.Add]);

    private static FamilyGame NewGame(params (string, int)[] players) =>
        new(Settings, players, new TaskGenerator(new Random(5)));

    private static void Answer(FamilyGame game, bool correct)
    {
        var answer = game.Current.Session.CurrentTask.Answer;
        game.Submit(correct ? answer : answer + 1);
        game.NextTurn();
    }

    [Fact]
    public void Players_take_turns_in_order_and_wrap_around()
    {
        var game = NewGame(("Anna", 1), ("Ben", 1), ("Cleo", 1));
        var order = new List<string>();
        for (var i = 0; i < 7; i++)
        {
            order.Add(game.Current.Name);
            Answer(game, true);
        }

        Assert.Equal(["Anna", "Ben", "Cleo", "Anna", "Ben", "Cleo", "Anna"], order);
        Assert.Equal(8, game.TurnNumber);
        Assert.Equal(3, game.Round);
    }

    [Fact]
    public void Each_player_has_their_own_level_and_range()
    {
        var game = NewGame(("Child", 1), ("Grandpa", 10));
        Assert.Equal(1, game.Players[0].Session.Level);
        Assert.Equal(10, game.Players[1].Session.Level);
        Assert.InRange(game.Players[0].Session.TaskSettings.Max, 1, 11);
        Assert.Equal(100, game.Players[1].Session.TaskSettings.Max);
    }

    [Fact]
    public void Answers_only_count_for_the_current_player()
    {
        var game = NewGame(("Anna", 1), ("Ben", 1));
        for (var turn = 0; turn < 6; turn++)
            Answer(game, correct: game.Current.Name == "Anna");

        var anna = game.Players[0].Session;
        var ben = game.Players[1].Session;
        Assert.Equal((3, 0), (anna.CorrectCount, anna.WrongCount));
        Assert.Equal((0, 3), (ben.CorrectCount, ben.WrongCount));
        Assert.Equal(2, anna.Level); // 3 correct in a row
        Assert.Equal(1, ben.Level);  // cannot go below 1
    }

    [Fact]
    public void Ranking_orders_by_correct_then_accuracy()
    {
        var game = NewGame(("Anna", 1), ("Ben", 1), ("Cleo", 1));
        // Round 1: A right, B right, C wrong. Round 2: A wrong, B right, C right.
        foreach (var correct in new[] { true, true, false, false, true, true })
            Answer(game, correct);

        Assert.Equal(["Ben", "Anna", "Cleo"], game.Ranking.Select(p => p.Name));
        Assert.Equal(["Ben"], game.Winners.Select(p => p.Name));
    }

    [Fact]
    public void Equal_scores_are_a_tie()
    {
        var game = NewGame(("Anna", 1), ("Ben", 1));
        Answer(game, true);
        Answer(game, true);
        Assert.Equal(["Anna", "Ben"], game.Winners.Select(p => p.Name));
    }

    [Fact]
    public void No_winner_without_correct_answers()
    {
        var game = NewGame(("Anna", 1), ("Ben", 1));
        Answer(game, false);
        Assert.Empty(game.Winners);
    }

    [Theory]
    [InlineData(new[] { "Anna", "Ben" }, true)]
    [InlineData(new[] { "Anna", "Ben", "Cleo", "Dan" }, true)]
    [InlineData(new[] { "Anna" }, false)]
    [InlineData(new[] { "A", "B", "C", "D", "E" }, false)]
    [InlineData(new[] { "Anna", " " }, false)]
    [InlineData(new[] { "Anna", "anna " }, false)]
    [InlineData(new[] { "Anna", "A very long name for a player" }, false)]
    public void Player_names_are_validated(string[] names, bool valid) =>
        Assert.Equal(valid, FamilyGame.ValidatePlayers(names) is null);

    [Fact]
    public void Names_are_trimmed()
    {
        var game = NewGame(("  Anna ", 1), ("Ben", 1));
        Assert.Equal("Anna", game.Players[0].Name);
    }

    [Fact]
    public void Invalid_players_throw() =>
        Assert.Throws<ArgumentException>(() => NewGame(("Anna", 1)));

    [Fact]
    public void Resume_level_is_per_player()
    {
        SessionRecord Rec(string? player, int level) =>
            new(DateTime.Now, TimeSpan.Zero, 1, 100, [Operation.Add], 1, 0, 1, level, player);
        var records = new[] { Rec(null, 7), Rec("Anna", 4), Rec("Ben", 9), Rec("anna", 5) };

        Assert.Equal(5, HistoryStore.ResumeLevel(records, Settings, "Anna"));
        Assert.Equal(9, HistoryStore.ResumeLevel(records, Settings, "BEN"));
        Assert.Equal(7, HistoryStore.ResumeLevel(records, Settings));
        Assert.Equal(1, HistoryStore.ResumeLevel(records, Settings, "Cleo"));
    }

    [Fact]
    public void Record_keeps_the_player_name()
    {
        var game = NewGame(("Anna", 3), ("Ben", 1));
        Answer(game, true);
        var record = SessionRecord.FromSession(game.Players[0].Session, game.Players[0].Name);
        Assert.Equal("Anna", record.Player);
        Assert.Equal(1, record.Correct);
        Assert.Equal(3, record.StartLevel);
    }
}
