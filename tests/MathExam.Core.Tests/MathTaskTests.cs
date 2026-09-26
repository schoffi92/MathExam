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

    [Theory]
    [InlineData(3, 2, 9, HiddenPart.Result, "3² = ?")]
    [InlineData(3, 2, 9, HiddenPart.Left, "?² = 9")]
    [InlineData(2, 3, 8, HiddenPart.Right, "2^? = 8")]
    [InlineData(-3, 3, -27, HiddenPart.Result, "(-3)³ = ?")]
    public void Power_display(long @base, long exponent, long result, HiddenPart hidden, string display) =>
        Assert.Equal(display, new MathTask(@base, Operation.Power, exponent, result, hidden).ToDisplayString());

    [Theory]
    [InlineData(2, 49, 7, HiddenPart.Result, "√49 = ?")]
    [InlineData(2, 49, 7, HiddenPart.Right, "√? = 7")]
    [InlineData(2, 49, 7, HiddenPart.Left, "?√49 = 7")]
    [InlineData(3, -8, -2, HiddenPart.Result, "³√-8 = ?")]
    public void Root_display(long degree, long radicand, long root, HiddenPart hidden, string display) =>
        Assert.Equal(display, new MathTask(degree, Operation.Root, radicand, root, hidden).ToDisplayString());

    [Fact]
    public void Exponent_and_degree_are_superscript_parts()
    {
        var power = new MathTask(2, Operation.Power, 5, 32, HiddenPart.Right).ToParts();
        Assert.Equal([new("2"), new("?", IsSuperscript: true), new(" = 32")], power);
        var root = new MathTask(3, Operation.Root, 27, 3, HiddenPart.Result).ToParts();
        Assert.Equal([new("3", IsSuperscript: true), new("√27 = ?")], root);
    }

    [Fact]
    public void Hidden_base_of_an_even_power_accepts_both_signs()
    {
        var task = new MathTask(3, Operation.Power, 2, 9, HiddenPart.Left);
        Assert.True(task.IsCorrect(3));
        Assert.True(task.IsCorrect(-3));
        Assert.False(task.IsCorrect(9));

        var odd = new MathTask(2, Operation.Power, 3, 8, HiddenPart.Left);
        Assert.False(odd.IsCorrect(-2));
    }
}
