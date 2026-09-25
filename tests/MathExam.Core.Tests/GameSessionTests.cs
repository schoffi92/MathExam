using MathExam.Core;

namespace MathExam.Core.Tests;

public class GameSessionTests
{
    private static GameSession NewSession() =>
        new(new GameSettings(1, 10, Enum.GetValues<Operation>()), new TaskGenerator(new Random(1)));

    [Fact]
    public void New_session_starts_at_task_one()
    {
        var session = NewSession();
        Assert.Equal(1, session.TaskNumber);
        Assert.Equal(0, session.AnsweredCount);
        Assert.False(session.IsCurrentAnswered);
        Assert.True(session.IsRunning);
    }

    [Fact]
    public void Submit_and_next_update_counters()
    {
        var session = NewSession();

        Assert.True(session.Submit(session.CurrentTask.Answer));
        Assert.True(session.IsCurrentAnswered);
        session.NextTask();

        Assert.False(session.Submit(session.CurrentTask.Answer + 1));
        session.NextTask();

        Assert.Equal(3, session.TaskNumber);
        Assert.Equal(1, session.CorrectCount);
        Assert.Equal(1, session.WrongCount);
        Assert.Equal(2, session.AnsweredCount);
    }

    [Fact]
    public void Submitting_twice_throws()
    {
        var session = NewSession();
        session.Submit(0);
        Assert.Throws<InvalidOperationException>(() => session.Submit(0));
    }

    [Fact]
    public void Stop_stops_the_clock()
    {
        var session = NewSession();
        session.Stop();
        Assert.False(session.IsRunning);
    }
}
