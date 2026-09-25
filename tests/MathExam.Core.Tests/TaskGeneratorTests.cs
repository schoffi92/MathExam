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
                _ => throw new InvalidOperationException(),
            };
            Assert.Equal(expected, t.Result);
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
    public void All_hidden_parts_occur()
    {
        var hidden = Generate(new GameSettings(1, 10, AllOps)).Select(t => t.Hidden).ToHashSet();
        Assert.Equal(Enum.GetValues<HiddenPart>().ToHashSet(), hidden);
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

    [Fact]
    public void Invalid_settings_throw()
    {
        var generator = new TaskGenerator();
        Assert.Throws<ArgumentException>(() => generator.Next(new GameSettings(10, 1, AllOps)));
    }
}
