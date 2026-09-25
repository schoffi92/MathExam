using MathExam.Core;

namespace MathExam.Core.Tests;

public sealed class HistoryStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "MathExamTests", Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_dir, "sub", "history.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    private static SessionRecord Record(int correct, int min = 1, int max = 10, int? endLevel = null) =>
        new(new DateTime(2026, 9, 25, 10, 0, 0), TimeSpan.FromMinutes(3), min, max,
            [Operation.Add, Operation.Divide], correct, 2, endLevel is null ? null : 1, endLevel);

    [Fact]
    public void Missing_file_means_empty_history() =>
        Assert.Empty(new HistoryStore(FilePath).Load());

    [Fact]
    public void Records_round_trip_in_order()
    {
        var store = new HistoryStore(FilePath);
        store.Add(Record(5));
        store.Add(Record(7, endLevel: 4));

        var loaded = new HistoryStore(FilePath).Load();
        Assert.Equal(2, loaded.Count);
        Assert.Equal(5, loaded[0].Correct);
        Assert.Null(loaded[0].EndLevel);
        Assert.Equal(7, loaded[1].Correct);
        Assert.Equal(4, loaded[1].EndLevel);
        Assert.Equal(TimeSpan.FromMinutes(3), loaded[1].Duration);
        Assert.Equal([Operation.Add, Operation.Divide], loaded[1].Operations);
        Assert.Null(loaded[1].Player);
    }

    [Fact]
    public void Player_name_round_trips_and_old_files_without_it_still_load()
    {
        var store = new HistoryStore(FilePath);
        store.Add(Record(3) with { Player = "Anna" });
        Assert.Equal("Anna", store.Load()[0].Player);

        // A file written before family mode existed has no "Player" property.
        File.WriteAllText(FilePath, """
            [{"StartedAt":"2026-09-25T10:00:00","Duration":"00:03:00","Min":1,"Max":10,
              "Operations":["Add"],"Correct":5,"Wrong":1,"StartLevel":null,"EndLevel":null}]
            """);
        var old = store.Load();
        Assert.Single(old);
        Assert.Null(old[0].Player);
        Assert.Equal(5, old[0].Correct);
    }

    [Fact]
    public void Corrupt_file_is_backed_up_and_ignored()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, "{ not json");

        var store = new HistoryStore(FilePath);
        Assert.Empty(store.Load());
        Assert.Equal("{ not json", File.ReadAllText(FilePath + ".bak"));

        store.Add(Record(1));
        Assert.Single(store.Load());
    }

    [Fact]
    public void Resume_level_uses_latest_adaptive_game_with_same_range()
    {
        var settings = new GameSettings(1, 10, [Operation.Add]);
        var records = new[]
        {
            Record(1, endLevel: 3),
            Record(1, endLevel: 6),
            Record(1),                        // not adaptive
            Record(1, min: 1, max: 20, endLevel: 9), // other range
        };
        Assert.Equal(6, HistoryStore.ResumeLevel(records, settings));
        Assert.Equal(DifficultyAdjuster.MinLevel, HistoryStore.ResumeLevel([], settings));
    }

    [Fact]
    public void Record_from_session_captures_results()
    {
        var session = new GameSession(new GameSettings(2, 9, [Operation.Multiply]), new TaskGenerator(new Random(1)), new DifficultyAdjuster(4));
        session.Submit(session.CurrentTask.Answer);
        session.Stop();

        var record = SessionRecord.FromSession(session);
        Assert.Equal(2, record.Min);
        Assert.Equal(9, record.Max);
        Assert.Equal(1, record.Correct);
        Assert.Equal(0, record.Wrong);
        Assert.Equal(4, record.StartLevel);
        Assert.Equal(4, record.EndLevel);
        Assert.True(record.IsAdaptive);
    }
}
