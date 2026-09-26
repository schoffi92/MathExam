using System.Globalization;
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
    public void Missing_operator_accepts_every_operator_that_makes_the_equation_true()
    {
        var task = new MathTask(2, Operation.Multiply, 2, 4, HiddenPart.Operator);
        Assert.Equal("2 ? 2 = 4", task.ToDisplayString());
        Assert.Equal("2 × 2 = 4", task.ToDisplayString(revealAnswer: true));
        Assert.True(task.IsCorrect(Operation.Multiply));
        Assert.True(task.IsCorrect(Operation.Add));
        Assert.False(task.IsCorrect(Operation.Subtract));
        Assert.False(task.IsCorrect(4));

        var division = new MathTask(18, Operation.Divide, 3, 6, HiddenPart.Operator);
        Assert.True(division.IsCorrect(Operation.Divide));
        Assert.False(division.IsCorrect(Operation.Multiply));
        Assert.False(new MathTask(5, Operation.Add, 0, 5, HiddenPart.Operator).IsCorrect(Operation.Divide));
    }

    [Fact]
    public void Fraction_tasks_show_fractions_and_accept_any_equal_answer()
    {
        // 1/2 + 1/4 = 3/4, stored over 4.
        var task = new MathTask(2, Operation.Add, 1, 3, HiddenPart.Result, NumberKind.Fraction, 4);
        Assert.Equal("1/2 + 1/4 = ?", task.ToDisplayString());
        Assert.Equal("3/4", task.AnswerText);
        foreach (var text in new[] { "3/4", "6/8", "0.75", "0,75" })
        {
            Assert.True(Rational.TryParse(text, out var answer));
            Assert.True(task.IsCorrect(answer), text);
        }
        Assert.False(task.IsCorrect(new Rational(1, 2)));
        Assert.Equal("1 1/4 − 1/2 = 3/4", new MathTask(5, Operation.Subtract, 2, 3, HiddenPart.Left, NumberKind.Fraction, 4)
            .ToDisplayString(revealAnswer: true));
    }

    [Fact]
    public void Decimal_tasks_show_tenths_in_the_current_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var task = new MathTask(12, Operation.Add, 5, 17, HiddenPart.Result, NumberKind.Decimal, 10);
            Assert.Equal("1.2 + 0.5 = ?", task.ToDisplayString());
            Assert.True(task.IsCorrect(new Rational(17, 10)));
            Assert.False(task.IsCorrect(17));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
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
