using MathExam.Core.Resources;

namespace MathExam.Core;

/// <summary>One player in a family game, with their own adaptive session.</summary>
public sealed record FamilyPlayer(string Name, GameSession Session);

/// <summary>
/// A turn-based game for several players on one computer. Every player has their own
/// <see cref="GameSession"/> with its own adaptive level, so players of different ages can play together;
/// the players take turns of <see cref="TasksPerTurn"/> tasks each.
/// </summary>
public sealed class FamilyGame
{
    public const int MinPlayers = 2;
    public const int MaxPlayers = 4;
    public const int MaxNameLength = 20;
    public const int MaxTasksPerTurn = 10;

    private readonly List<FamilyPlayer> _players;

    /// <param name="players">Names and the adaptive level each player starts at, in turn order.</param>
    /// <param name="tasksPerTurn">How many tasks a player answers in a row before the turn passes on.</param>
    public FamilyGame(GameSettings settings, IEnumerable<(string Name, int StartLevel)> players,
        TaskGenerator? generator = null, int tasksPerTurn = 1)
    {
        var list = players.ToList();
        var error = ValidatePlayers(list.Select(p => p.Name));
        if (error is not null)
            throw new ArgumentException(error, nameof(players));
        ArgumentOutOfRangeException.ThrowIfLessThan(tasksPerTurn, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tasksPerTurn, MaxTasksPerTurn);

        generator ??= new TaskGenerator();
        Settings = settings;
        var startedAt = DateTime.Now; // shared, so the history can tell which records form one game
        _players = list
            .Select(p => new FamilyPlayer(p.Name.Trim(),
                new GameSession(settings, generator, new DifficultyAdjuster(p.StartLevel), startedAt)))
            .ToList();
        TasksPerTurn = tasksPerTurn;
        TurnNumber = 1;
        TaskInTurn = 1;
    }

    public GameSettings Settings { get; }
    public IReadOnlyList<FamilyPlayer> Players => _players;
    public int CurrentIndex { get; private set; }
    public FamilyPlayer Current => _players[CurrentIndex];

    public int TasksPerTurn { get; }

    /// <summary>Which of the current player's <see cref="TasksPerTurn"/> tasks this is, starting at 1.</summary>
    public int TaskInTurn { get; private set; }

    public bool IsLastTaskOfTurn => TaskInTurn == TasksPerTurn;

    /// <summary>Counts every turn of every player, starting at 1.</summary>
    public int TurnNumber { get; private set; }

    /// <summary>A round is complete when every player has had one turn.</summary>
    public int Round => (TurnNumber - 1) / _players.Count + 1;

    public bool Submit(long answer) => Current.Session.Submit(answer);

    public bool Submit(Rational answer) => Current.Session.Submit(answer);

    /// <summary>Answers a missing-operator task for the current player.</summary>
    public bool SubmitOperator(Operation answer) => Current.Session.SubmitOperator(answer);

    /// <summary>
    /// Prepares the current player's next task. After their last task of the turn, the turn passes to the next player.
    /// </summary>
    public void Next()
    {
        Current.Session.NextTask();
        if (!IsLastTaskOfTurn)
        {
            TaskInTurn++;
            return;
        }
        TaskInTurn = 1;
        CurrentIndex = (CurrentIndex + 1) % _players.Count;
        TurnNumber++;
    }

    public void Stop()
    {
        foreach (var player in _players)
            player.Session.Stop();
    }

    /// <summary>Players ordered by correct answers, then accuracy; ties keep turn order.</summary>
    public IReadOnlyList<FamilyPlayer> Ranking => _players
        .OrderByDescending(p => p.Session.CorrectCount)
        .ThenByDescending(p => Accuracy(p.Session))
        .ToList();

    /// <summary>The players sharing first place (more than one means a tie); empty if nobody answered correctly.</summary>
    public IReadOnlyList<FamilyPlayer> Winners
    {
        get
        {
            var best = _players.Max(p => p.Session.CorrectCount);
            if (best == 0)
                return [];
            var bestAccuracy = _players.Where(p => p.Session.CorrectCount == best).Max(p => Accuracy(p.Session));
            return Ranking.Where(p => p.Session.CorrectCount == best && Accuracy(p.Session) == bestAccuracy).ToList();
        }
    }

    /// <summary>Returns a user-facing error message in the current UI language, or null when the names are valid.</summary>
    public static string? ValidatePlayers(IEnumerable<string> names)
    {
        var trimmed = names.Select(n => n?.Trim() ?? "").ToList();
        if (trimmed.Count is < MinPlayers or > MaxPlayers)
            return string.Format(Strings.Players_Count, MinPlayers, MaxPlayers);
        if (trimmed.Any(n => n.Length == 0))
            return Strings.Players_EmptyName;
        if (trimmed.Any(n => n.Length > MaxNameLength))
            return string.Format(Strings.Players_NameTooLong, MaxNameLength);
        if (trimmed.Distinct(StringComparer.OrdinalIgnoreCase).Count() != trimmed.Count)
            return Strings.Players_DuplicateName;
        return null;
    }

    private static double Accuracy(GameSession s) => s.AnsweredCount == 0 ? 0 : (double)s.CorrectCount / s.AnsweredCount;
}
