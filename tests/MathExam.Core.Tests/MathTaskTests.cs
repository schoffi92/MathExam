using MathExam.Core;

namespace MathExam.Core.Tests;

public class MathTaskTests
{
    [Theory]
    [InlineData(HiddenPart.Result, "100 × 100 = ?", 10000)]
    [InlineData(HiddenPart.Right, "100 × ? = 10000", 100)]
    [InlineData(HiddenPart.Left, "? × 100 = 10000", 100)]
    public void Display_and_answer_follow_hidden_part(HiddenPart hidden, string display, long answer)
    {
        var task = new MathTask(100, Operation.Multiply, 100, 10000, hidden);
        Assert.Equal(display, task.ToDisplayString());
        Assert.Equal(answer, task.Answer);
        Assert.True(task.IsCorrect(answer));
        Assert.False(task.IsCorrect(answer + 1));
    }
}
