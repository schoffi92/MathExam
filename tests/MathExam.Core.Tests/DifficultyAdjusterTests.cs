using MathExam.Core;

namespace MathExam.Core.Tests;

public class DifficultyAdjusterTests
{
    private static readonly Operation[] AllOps = Enum.GetValues<Operation>();

    [Fact]
    public void Level_rises_after_three_correct_answers()
    {
        var d = new DifficultyAdjuster();
        Assert.Equal(0, d.RecordAnswer(true));
        Assert.Equal(0, d.RecordAnswer(true));
        Assert.Equal(1, d.RecordAnswer(true));
        Assert.Equal(2, d.Level);
    }

    [Fact]
    public void Level_falls_after_two_wrong_answers()
    {
        var d = new DifficultyAdjuster(5);
        Assert.Equal(0, d.RecordAnswer(false));
        Assert.Equal(-1, d.RecordAnswer(false));
        Assert.Equal(4, d.Level);
    }

    [Fact]
    public void A_wrong_answer_resets_the_correct_streak()
    {
        var d = new DifficultyAdjuster();
        d.RecordAnswer(true);
        d.RecordAnswer(true);
        d.RecordAnswer(false);
        d.RecordAnswer(true);
        d.RecordAnswer(true);
        Assert.Equal(1, d.Level);
    }

    [Fact]
    public void Level_is_clamped()
    {
        Assert.Equal(DifficultyAdjuster.MaxLevel, new DifficultyAdjuster(99).Level);
        Assert.Equal(DifficultyAdjuster.MinLevel, new DifficultyAdjuster(-3).Level);

        var top = new DifficultyAdjuster(DifficultyAdjuster.MaxLevel);
        for (var i = 0; i < 3; i++) top.RecordAnswer(true);
        Assert.Equal(DifficultyAdjuster.MaxLevel, top.Level);

        var bottom = new DifficultyAdjuster();
        for (var i = 0; i < 2; i++) bottom.RecordAnswer(false);
        Assert.Equal(DifficultyAdjuster.MinLevel, bottom.Level);
    }

    [Theory]
    [InlineData(1, 10, 1, 1, 2)]
    [InlineData(1, 10, 5, 1, 6)]
    [InlineData(1, 10, 10, 1, 10)]
    [InlineData(-20, 20, 1, -2, 2)]
    [InlineData(-20, 20, 10, -20, 20)]
    [InlineData(-10, -1, 1, -2, -1)]
    [InlineData(0, 100, 3, 0, 30)]
    public void Range_grows_from_the_easiest_number(int min, int max, int level, int expectedMin, int expectedMax)
    {
        var s = DifficultyAdjuster.ForLevel(new GameSettings(min, max, AllOps), level);
        Assert.Equal(expectedMin, s.Min);
        Assert.Equal(expectedMax, s.Max);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 0)]
    [InlineData(0, 1000)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void Valid_settings_stay_valid_on_every_level(int min, int max)
    {
        // Unit conversion needs a number of at least 1, so it is left out of ranges without one.
        var settings = new GameSettings(min, max, AllOps.Where(o => o != Operation.Convert || max >= 1));
        Assert.True(settings.IsValid);
        for (var level = DifficultyAdjuster.MinLevel; level <= DifficultyAdjuster.MaxLevel; level++)
            Assert.True(DifficultyAdjuster.ForLevel(settings, level).IsValid);
    }

    [Fact]
    public void Adaptive_session_generates_tasks_within_the_current_range()
    {
        var settings = new GameSettings(1, 100, [Operation.Add]);
        var session = new GameSession(settings, new TaskGenerator(new Random(3)), new DifficultyAdjuster());

        for (var i = 0; i < 60; i++)
        {
            var range = session.TaskSettings;
            Assert.InRange(session.CurrentTask.Left, range.Min, range.Max);
            Assert.InRange(session.CurrentTask.Right, range.Min, range.Max);
            session.Submit(session.CurrentTask.Answer);
            session.NextTask();
        }

        Assert.Equal(DifficultyAdjuster.MaxLevel, session.Level);
        Assert.Equal(1, session.StartLevel);
    }

    [Fact]
    public void Session_reports_level_changes()
    {
        var session = new GameSession(new GameSettings(1, 10, AllOps), new TaskGenerator(new Random(1)), new DifficultyAdjuster());
        for (var i = 0; i < 2; i++)
        {
            session.Submit(session.CurrentTask.Answer);
            Assert.Equal(0, session.LastLevelChange);
            session.NextTask();
        }
        session.Submit(session.CurrentTask.Answer);
        Assert.Equal(1, session.LastLevelChange);
        session.NextTask();
        Assert.Equal(0, session.LastLevelChange);
    }
}
