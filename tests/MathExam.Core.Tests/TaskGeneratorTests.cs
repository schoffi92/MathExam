using MathExam.Core;

namespace MathExam.Core.Tests;

public class TaskGeneratorTests
{
    private static readonly Operation[] AllOps = Enum.GetValues<Operation>();

    private static IEnumerable<MathTask> Generate(GameSettings settings, int count = 2000, int seed = 42)
    {
        var generator = new TaskGenerator(new Random(seed));
        for (var i = 0; i < count; i++)
            yield return generator.Next(settings);
    }

    private static long Pow(long @base, long exponent)
    {
        long result = 1;
        for (var i = 0; i < exponent; i++)
            result *= @base;
        return result;
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(0, 100)]
    [InlineData(-20, 20)]
    [InlineData(5, 5)]
    public void Every_task_is_arithmetically_valid(int min, int max)
    {
        foreach (var t in Generate(new GameSettings(min, max, AllOps)))
        {
            var expected = t.Op switch
            {
                Operation.Add => t.Left + t.Right,
                Operation.Subtract => t.Left - t.Right,
                Operation.Multiply => t.Left * t.Right,
                Operation.Divide => t.Left / t.Right,
                Operation.Power => Pow(t.Left, t.Right),
                Operation.Root => t.Result,
                _ => throw new InvalidOperationException(),
            };
            Assert.Equal(expected, t.Result);
            if (t.Op == Operation.Root)
                Assert.Equal(t.Right, Pow(t.Result, t.Left));
        }
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(-10, 10)]
    public void Division_is_exact_and_never_by_zero(int min, int max)
    {
        foreach (var t in Generate(new GameSettings(min, max, [Operation.Divide])))
        {
            Assert.NotEqual(0, t.Right);
            Assert.Equal(0, t.Left % t.Right);
            Assert.InRange(t.Right, min, max);
            Assert.InRange(t.Result, min, max);
        }
    }

    [Fact]
    public void Only_enabled_operations_appear()
    {
        var tasks = Generate(new GameSettings(1, 10, [Operation.Add, Operation.Multiply])).ToList();
        Assert.All(tasks, t => Assert.Contains(t.Op, new[] { Operation.Add, Operation.Multiply }));
        Assert.Contains(tasks, t => t.Op == Operation.Add);
        Assert.Contains(tasks, t => t.Op == Operation.Multiply);
    }

    [Theory]
    [InlineData(Operation.Add)]
    [InlineData(Operation.Subtract)]
    [InlineData(Operation.Multiply)]
    public void Operands_are_within_range(Operation op)
    {
        foreach (var t in Generate(new GameSettings(3, 12, [op])))
        {
            Assert.InRange(t.Left, 3, 12);
            Assert.InRange(t.Right, 3, 12);
        }
    }

    [Fact]
    public void Subtraction_is_non_negative_for_non_negative_range()
    {
        Assert.All(Generate(new GameSettings(0, 50, [Operation.Subtract])), t => Assert.True(t.Result >= 0));
    }

    [Fact]
    public void All_numbers_can_be_hidden_but_not_the_operator_by_default()
    {
        var hidden = Generate(new GameSettings(1, 10, AllOps)).Select(t => t.Hidden).ToHashSet();
        Assert.Equal([HiddenPart.Left, HiddenPart.Right, HiddenPart.Result], hidden.Order());
    }

    [Fact]
    public void Missing_operator_hides_only_basic_operators_and_the_task_stays_true()
    {
        var tasks = Generate(new GameSettings(1, 10, AllOps, missingOperator: true)).ToList();
        var hiddenOperator = tasks.Where(t => t.Hidden == HiddenPart.Operator).ToList();
        Assert.NotEmpty(hiddenOperator);
        Assert.All(hiddenOperator, t =>
        {
            Assert.True(t.Op.IsBasic());
            Assert.True(t.IsCorrect(t.Op));
            Assert.Equal(t.Op.Symbol(), t.AnswerText);
        });
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 3)]
    [InlineData(-2, 2)]
    public void Fraction_tasks_add_or_subtract_fractions_within_the_range(int min, int max)
    {
        var tasks = Generate(new GameSettings(min, max, [Operation.Add, Operation.Subtract], NumberKind.Fraction)).ToList();
        foreach (var t in tasks)
        {
            Assert.Equal(NumberKind.Fraction, t.Numbers);
            Assert.InRange(t.Denominator, 2, 24);
            Assert.Equal(t.Op == Operation.Add ? t.Left + t.Right : t.Left - t.Right, t.Result);
            Assert.InRange(t.Left, min * t.Denominator, max * t.Denominator);
            Assert.InRange(t.Right, min * t.Denominator, max * t.Denominator);
            Assert.True(t.Left % t.Denominator != 0 || t.Right % t.Denominator != 0, $"{t} has no fraction");
            if (min >= 0)
                Assert.True(t.Result >= 0);
        }
        // Both like and unlike denominators occur.
        Assert.Contains(tasks, t => new Rational(t.Left, t.Denominator).Denominator != new Rational(t.Right, t.Denominator).Denominator);
    }

    [Fact]
    public void Decimal_tasks_use_tenths()
    {
        foreach (var t in Generate(new GameSettings(0, 5, [Operation.Add], NumberKind.Decimal)))
        {
            Assert.Equal(TaskGenerator.DecimalDenominator, t.Denominator);
            Assert.Equal(t.Left + t.Right, t.Result);
            Assert.InRange(t.Left, 0, 50);
        }
    }

    [Fact]
    public void A_range_without_fractions_still_generates_tasks()
    {
        Assert.All(Generate(new GameSettings(0, 0, [Operation.Add], NumberKind.Fraction), count: 20), t => Assert.Equal(0, t.Result));
    }

    [Fact]
    public void Ambiguous_zero_operands_are_never_hidden()
    {
        foreach (var t in Generate(new GameSettings(0, 3, [Operation.Multiply, Operation.Divide])))
        {
            if (t.Op == Operation.Multiply && t.Hidden == HiddenPart.Left) Assert.NotEqual(0, t.Right);
            if (t.Op == Operation.Multiply && t.Hidden == HiddenPart.Right) Assert.NotEqual(0, t.Left);
            if (t.Op == Operation.Divide && t.Hidden == HiddenPart.Right) Assert.NotEqual(0, t.Left);
        }
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(-20, 20)]
    [InlineData(-5000, -900)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void Powers_use_small_exponents_and_stay_small_beyond_squares(int min, int max)
    {
        foreach (var t in Generate(new GameSettings(min, max, [Operation.Power])))
        {
            Assert.InRange(t.Left, Math.Max(min, -GameSettings.PowerBaseLimit), Math.Min(max, GameSettings.PowerBaseLimit));
            Assert.InRange(t.Right, 2, TaskGenerator.MaxExponent);
            if (t.Right > 2)
                Assert.InRange(Math.Abs(t.Result), 0, TaskGenerator.MaxHigherPower);
        }
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(-20, 20)]
    [InlineData(-1000, -500)]
    public void Roots_are_whole_and_negative_numbers_only_get_odd_roots(int min, int max)
    {
        var tasks = Generate(new GameSettings(min, max, [Operation.Root])).ToList();
        foreach (var t in tasks)
        {
            Assert.InRange(t.Result, min, max);
            Assert.InRange(t.Left, 2, TaskGenerator.MaxExponent);
            Assert.Equal(t.Right, Pow(t.Result, t.Left));
            if (t.Right < 0)
                Assert.Equal(1, t.Left % 2);
        }
        if (min >= 0)
            Assert.Contains(tasks, t => t.Left == 2);
    }

    [Fact]
    public void Ambiguous_power_and_root_parts_are_never_hidden()
    {
        foreach (var t in Generate(new GameSettings(-1, 1, [Operation.Power, Operation.Root])))
        {
            if (t.Op == Operation.Power) Assert.NotEqual(HiddenPart.Right, t.Hidden);
            if (t.Op == Operation.Root) Assert.NotEqual(HiddenPart.Left, t.Hidden);
        }
    }

    [Fact]
    public void Invalid_settings_throw()
    {
        var generator = new TaskGenerator();
        Assert.Throws<ArgumentException>(() => generator.Next(new GameSettings(10, 1, AllOps)));
    }
}
