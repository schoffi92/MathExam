using MathExam.Core;

namespace MathExam.Core.Tests;

/// <summary>A clock the test moves forward by hand.</summary>
internal sealed class ManualClock : TimeProvider
{
    private long _ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => _ticks;
    public void Advance(TimeSpan time) => _ticks += time.Ticks;
}

public class TimedAndStatsTests
{
    private static readonly GameSettings Settings = new(1, 10, Enum.GetValues<Operation>());

    [Fact]
    public void Timed_session_counts_down_and_never_reports_more_than_its_limit()
    {
        var clock = new ManualClock();
        var session = new GameSession(Settings, new TaskGenerator(new Random(1)), timeLimit: TimeSpan.FromSeconds(60), clock: clock);

        clock.Advance(TimeSpan.FromSeconds(45));
        Assert.Equal(TimeSpan.FromSeconds(15), session.TimeLeft);
        Assert.False(session.IsTimeUp);

        clock.Advance(TimeSpan.FromSeconds(20));
        Assert.True(session.IsTimeUp);
        Assert.Equal(TimeSpan.FromSeconds(60), session.Elapsed);
        Assert.Equal(TimeSpan.Zero, session.TimeLeft);
    }

    [Fact]
    public void Stop_freezes_the_elapsed_time()
    {
        var clock = new ManualClock();
        var session = new GameSession(Settings, new TaskGenerator(new Random(1)), clock: clock);
        clock.Advance(TimeSpan.FromSeconds(30));
        session.Stop();
        clock.Advance(TimeSpan.FromSeconds(30));
        Assert.Equal(TimeSpan.FromSeconds(30), session.Elapsed);
        Assert.False(session.IsRunning);
        Assert.False(session.IsTimed);
        Assert.Null(session.TimeLeft);
    }

    [Fact]
    public void Answers_are_counted_per_operation()
    {
        var session = new GameSession(Settings, new TaskGenerator(new Random(2)));
        var expected = new Dictionary<Operation, (int Correct, int Wrong)>();
        for (var i = 0; i < 40; i++)
        {
            var task = session.CurrentTask;
            var correct = i % 3 != 0;
            session.Submit(correct ? task.Answer : task.Answer + 1000);
            var (c, w) = expected.GetValueOrDefault(task.Op);
            expected[task.Op] = correct ? (c + 1, w) : (c, w + 1);
            session.NextTask();
        }

        Assert.Equal(expected.Keys.Order(), session.ByOperation.Keys.Order());
        foreach (var (op, (c, w)) in expected)
            Assert.Equal(new OperationStats(c, w), session.ByOperation[op]);
        Assert.Equal(session.CorrectCount, session.ByOperation.Values.Sum(s => s.Correct));
    }

    [Fact]
    public void Missing_operator_tasks_are_answered_with_an_operator()
    {
        var session = new GameSession(new GameSettings(1, 10, [Operation.Multiply], missingOperator: true),
            new TaskGenerator(new Random(4)));
        while (session.CurrentTask.Hidden != HiddenPart.Operator)
        {
            session.Submit(session.CurrentTask.Answer);
            session.NextTask();
        }
        var before = session.CorrectCount;
        Assert.True(session.SubmitOperator(Operation.Multiply));
        Assert.Equal(before + 1, session.CorrectCount);
    }

    private static SessionRecord Timed(int correct, int? limit = 60, string? player = null, int max = 10) =>
        new(DateTime.Now, TimeSpan.FromSeconds(60), 1, max, [Operation.Add], correct, 0, null, null, player,
            TimeLimitSeconds: limit);

    [Fact]
    public void Personal_best_compares_timed_solo_games_with_the_same_settings()
    {
        var history = new[]
        {
            Timed(12), Timed(15), Timed(30, limit: null), Timed(40, player: "Anna"), Timed(50, max: 20), Timed(9),
        };
        Assert.Equal(15, HistoryStore.PersonalBest(history, Timed(0)));
        Assert.Null(HistoryStore.PersonalBest([Timed(30, limit: null)], Timed(0)));
        Assert.Null(HistoryStore.PersonalBest(history, Timed(0, limit: null)));
    }

    [Fact]
    public void New_record_fields_survive_a_save_and_load()
    {
        var dir = Path.Combine(Path.GetTempPath(), "MathExamTests", Guid.NewGuid().ToString("N"));
        try
        {
            var store = new HistoryStore(Path.Combine(dir, "history.json"));
            var record = new SessionRecord(new DateTime(2026, 9, 26), TimeSpan.FromSeconds(60), 0, 1, [Operation.Add], 5, 1,
                null, null, null, new Dictionary<Operation, OperationStats> { [Operation.Add] = new(5, 1) }, 60,
                NumberKind.Fraction, MissingOperator: true);
            store.Add(record);

            var loaded = Assert.Single(store.Load());
            Assert.Equal((60, NumberKind.Fraction, true), (loaded.TimeLimitSeconds, loaded.Numbers, loaded.MissingOperator));
            Assert.Equal(new OperationStats(5, 1), loaded.ByOperation![Operation.Add]);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Totals_add_up_per_operation_and_skip_old_games()
    {
        SessionRecord Rec(Dictionary<Operation, OperationStats>? byOp) =>
            new(DateTime.Now, TimeSpan.FromSeconds(10), 1, 10, [Operation.Add], 1, 1, null, null, ByOperation: byOp);

        var totals = HistoryTotals.From(
        [
            Rec(new() { [Operation.Add] = new(3, 1), [Operation.Multiply] = new(1, 1) }),
            Rec(new() { [Operation.Add] = new(2, 0) }),
            Rec(null),
        ]);

        Assert.Equal([Operation.Add, Operation.Multiply], totals.ByOperation.Keys);
        Assert.Equal(new OperationStats(5, 1), totals.ByOperation[Operation.Add]);
        Assert.Equal(0.5, totals.ByOperation[Operation.Multiply].Accuracy);
    }

    [Fact]
    public void Resume_level_keeps_whole_numbers_and_fractions_apart()
    {
        var fractions = new SessionRecord(DateTime.Now, TimeSpan.Zero, 0, 1, [Operation.Add], 1, 0, 1, 7, Numbers: NumberKind.Fraction);
        Assert.Equal(7, HistoryStore.ResumeLevel([fractions], new GameSettings(0, 1, [Operation.Add], NumberKind.Fraction)));
        Assert.Equal(1, HistoryStore.ResumeLevel([fractions], new GameSettings(0, 1, [Operation.Add])));
    }
}
