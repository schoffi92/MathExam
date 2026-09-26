using MathExam.Core;

namespace MathExam.Core.Tests;

public sealed class ProfileTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "MathExamTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    private static readonly DateTime Today = new(2026, 9, 26, 15, 0, 0);

    private static SessionRecord Solo(string? profile, int answered = 10, DateTime? at = null, int? endLevel = null, int? taskLimit = null) =>
        new(at ?? Today, TimeSpan.FromMinutes(2), 1, 10, [Operation.Add], answered, 0, endLevel is null ? null : 1, endLevel,
            Profile: profile, TaskLimit: taskLimit);

    private static SessionRecord Family(string player, int answered = 4, DateTime? at = null) =>
        new(at ?? Today, TimeSpan.FromMinutes(1), 1, 10, [Operation.Add], answered, 0, 1, 1, player);

    [Fact]
    public void Profiles_round_trip_and_a_missing_or_corrupt_file_gives_none()
    {
        var store = new ProfileStore(Path.Combine(_dir, "profiles.json"));
        Assert.Empty(store.Load().Profiles);

        store.Save(new ProfileList([new Profile("Anna", 20), new Profile("Ben")], "Anna"));
        var loaded = store.Load();
        Assert.Equal([new Profile("Anna", 20), new Profile("Ben")], loaded.Profiles);
        Assert.Equal("Anna", loaded.Current);

        File.WriteAllText(store.FilePath, "not json");
        Assert.Empty(store.Load().Profiles);
    }

    [Theory]
    [InlineData("Cleo", true)]
    [InlineData("anna", false)]
    [InlineData("  ", false)]
    [InlineData("A name that is far too long", false)]
    public void New_names_must_be_unique_and_not_empty(string name, bool valid)
    {
        var list = new ProfileList([new Profile("Anna")]);
        Assert.Equal(valid, list.ValidateNewName(name) is null);
    }

    [Fact]
    public void Tasks_today_count_solo_games_of_the_profile_and_family_games_under_the_same_name()
    {
        var records = new[]
        {
            Solo("Anna", 10), Solo("anna", 5), Family("Anna", 4), Solo("Ben", 7), Solo(null, 9),
            Solo("Anna", 30, at: Today.AddDays(-1)),
        };
        Assert.Equal(19, PlayerStats.TasksOn(records, "Anna", DateOnly.FromDateTime(Today)));
    }

    [Fact]
    public void Resume_level_is_kept_per_profile()
    {
        var settings = new GameSettings(1, 10, [Operation.Add]);
        var records = new[] { Solo("Anna", endLevel: 6), Solo(null, endLevel: 3), Family("Anna") };
        Assert.Equal(6, HistoryStore.ResumeLevel(records, settings, profile: "anna"));
        Assert.Equal(3, HistoryStore.ResumeLevel(records, settings));
        Assert.Equal(1, HistoryStore.ResumeLevel(records, settings, profile: "Ben"));
        Assert.Equal(1, HistoryStore.ResumeLevel(records, settings, player: "Anna"));
    }

    [Fact]
    public void Personal_best_is_kept_per_profile()
    {
        SessionRecord Timed(string? profile, int correct) =>
            new(Today, TimeSpan.FromMinutes(1), 1, 10, [Operation.Add], correct, 0, null, null, TimeLimitSeconds: 60, Profile: profile);
        var history = new[] { Timed("Anna", 20), Timed("Ben", 30), Timed(null, 25) };
        Assert.Equal(20, HistoryStore.PersonalBest(history, Timed("Anna", 0)));
        Assert.Equal(25, HistoryStore.PersonalBest(history, Timed(null, 0)));
    }

    [Fact]
    public void Overview_lists_profiles_first_then_other_players_and_the_guest()
    {
        var profiles = new ProfileList([new Profile("Anna", 20), new Profile("Cleo", 10)]);
        var records = new[] { Solo("Anna", 12), Family("Grandpa", 8), Solo(null, 5), Solo("Anna", 6, at: Today.AddDays(-2)) };

        var rows = PlayerStats.Overview(records, profiles, DateOnly.FromDateTime(Today));

        Assert.Equal(["Anna", "Cleo", "Grandpa", null], rows.Select(r => r.Name));
        var anna = rows[0];
        Assert.Equal((2, 18, 12, 20), (anna.Games, anna.Tasks, anna.TasksToday, anna.DailyGoal));
        Assert.Equal(Today, anna.LastPlayed);
        Assert.Equal((0, (DateTime?)null), (rows[1].Games, rows[1].LastPlayed));
        Assert.Equal(5, rows[3].Tasks);
    }

    [Theory]
    [InlineData(null, null, null, GameMode.Practice)]
    [InlineData(null, 60, null, GameMode.Timed)]
    [InlineData(null, null, 20, GameMode.Test)]
    [InlineData("Anna", null, null, GameMode.Family)]
    public void Mode_is_derived_from_the_record(string? player, int? timeLimit, int? taskLimit, GameMode mode)
    {
        var record = new SessionRecord(Today, TimeSpan.Zero, 1, 10, [Operation.Add], 1, 0, null, null, player,
            TimeLimitSeconds: timeLimit, TaskLimit: taskLimit);
        Assert.Equal(mode, record.Mode);
    }

    [Fact]
    public void A_test_is_complete_after_its_tasks_and_keeps_the_mistakes()
    {
        var session = new GameSession(new GameSettings(1, 10, [Operation.Add]), new TaskGenerator(new Random(3)), taskLimit: 3);
        var wrongTask = session.CurrentTask;
        session.Submit(wrongTask.Answer + 1);
        session.NextTask();
        session.Submit(session.CurrentTask.Answer);
        Assert.False(session.IsTestComplete);
        session.NextTask();
        session.Submit(session.CurrentTask.Answer);

        Assert.True(session.IsTestComplete);
        var mistake = Assert.Single(session.Mistakes);
        Assert.Equal(wrongTask, mistake.Task);
        Assert.Equal((wrongTask.Answer + 1).ToString(), mistake.Given);

        var record = SessionRecord.FromSession(session, profile: "Anna");
        Assert.Equal((3, "Anna", GameMode.Test), (record.TaskLimit!.Value, record.Profile, record.Mode));
    }

    [Fact]
    public void Csv_names_the_player_of_solo_and_family_games_and_the_mode()
    {
        var writer = new StringWriter();
        HistoryCsv.Write(writer, [Solo("Anna", taskLimit: 10), Family("Grandpa")]);
        var lines = writer.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.StartsWith("Started,Player,Mode,", lines[0]);
        Assert.Contains(",Anna,Test,", lines[1]);
        Assert.Contains(",Grandpa,Family,", lines[2]);
    }
}
