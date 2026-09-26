using MathExam.Core;

namespace MathExam.Core.Tests;

public class ExplainerTests
{
    private static string? Explain(long l, Operation op, long r, long res, HiddenPart hidden = HiddenPart.Result) =>
        Explainer.Explain(new MathTask(l, op, r, res, hidden));

    [Theory]
    [InlineData(8, 7, "8 + 7 = 8 + 2 + 5 = 10 + 5 = 15")]
    [InlineData(7, 8, "8 + 7 = 8 + 2 + 5 = 10 + 5 = 15")]
    [InlineData(27, 45, "27 + 45 = 20 + 40 + 7 + 5 = 60 + 12 = 72")]
    [InlineData(20, 45, "20 + 45 = 20 + 40 + 5 = 60 + 5 = 65")]
    [InlineData(27, 5, "27 + 5 = 27 + 3 + 2 = 30 + 2 = 32")]
    [InlineData(23, 5, "23 + 5 = 20 + 3 + 5 = 20 + 8 = 28")]
    public void Addition_bridges_through_ten_or_adds_tens_and_ones(long l, long r, string expected) =>
        Assert.Equal(expected, Explain(l, Operation.Add, r, l + r));

    [Theory]
    [InlineData(2, 3)]
    [InlineData(20, 40)]
    [InlineData(27, 3)]
    [InlineData(-3, 5)]
    public void Easy_additions_have_no_explanation(long l, long r) => Assert.Null(Explain(l, Operation.Add, r, l + r));

    [Theory]
    [InlineData(52, 17, "52 − 17 = 52 − 10 − 7 = 42 − 7 = 35")]
    [InlineData(52, 7, "52 − 7 = 52 − 2 − 5 = 50 − 5 = 45")]
    public void Subtraction_takes_away_tens_then_ones(long l, long r, string expected) =>
        Assert.Equal(expected, Explain(l, Operation.Subtract, r, l - r));

    [Theory]
    [InlineData(12, 7, "12 × 7 = 10 × 7 + 2 × 7 = 70 + 14 = 84")]
    [InlineData(7, 12, "7 × 12 = 7 × 10 + 7 × 2 = 70 + 14 = 84")]
    [InlineData(7, 8, "7 × 8 = 7 × 5 + 7 × 3 = 35 + 21 = 56")]
    [InlineData(8, 4, "8 × 4 = 5 × 4 + 3 × 4 = 20 + 12 = 32")]
    public void Multiplication_splits_a_factor(long l, long r, string expected) =>
        Assert.Equal(expected, Explain(l, Operation.Multiply, r, l * r));

    [Fact]
    public void Division_goes_back_to_multiplication() =>
        Assert.Equal("7 × 8 = 56 → 56 ÷ 8 = 7", Explain(56, Operation.Divide, 8, 7));

    [Theory]
    [InlineData(Operation.Add, HiddenPart.Left, 7, 8, 15, "15 − 8 = 7")]
    [InlineData(Operation.Add, HiddenPart.Right, 7, 8, 15, "15 − 7 = 8")]
    [InlineData(Operation.Subtract, HiddenPart.Left, 15, 8, 7, "7 + 8 = 15")]
    [InlineData(Operation.Subtract, HiddenPart.Right, 15, 8, 7, "15 − 7 = 8")]
    [InlineData(Operation.Multiply, HiddenPart.Left, 7, 8, 56, "56 ÷ 8 = 7")]
    [InlineData(Operation.Divide, HiddenPart.Left, 56, 8, 7, "7 × 8 = 56")]
    [InlineData(Operation.Divide, HiddenPart.Right, 56, 8, 7, "56 ÷ 7 = 8")]
    [InlineData(Operation.Subtract, HiddenPart.Right, 5, -3, 8, "5 − 8 = -3")]
    [InlineData(Operation.Add, HiddenPart.Left, 5, -3, 2, "2 − (-3) = 5")]
    public void Hidden_operands_undo_the_operation(Operation op, HiddenPart hidden, long l, long r, long res, string expected) =>
        Assert.Equal(expected, Explain(l, op, r, res, hidden));

    [Theory]
    [InlineData(3, 4, 81, HiddenPart.Result, "3⁴ = 3 × 3 × 3 × 3 = 81")]
    [InlineData(-2, 3, -8, HiddenPart.Result, "(-2)³ = (-2) × (-2) × (-2) = -8")]
    [InlineData(7, 2, 49, HiddenPart.Left, "7 × 7 = 49")]
    [InlineData(2, 4, 16, HiddenPart.Right, "2 × 2 × 2 × 2 = 16 → 2⁴ = 16")]
    public void Powers_are_repeated_multiplication(long b, long e, long res, HiddenPart hidden, string expected) =>
        Assert.Equal(expected, Explain(b, Operation.Power, e, res, hidden));

    [Theory]
    [InlineData(2, 49, 7, HiddenPart.Result, "7 × 7 = 49 → √49 = 7")]
    [InlineData(3, 64, 4, HiddenPart.Left, "4 × 4 × 4 = 64 → ³√64 = 4")]
    public void Roots_go_back_to_powers(long degree, long radicand, long root, HiddenPart hidden, string expected) =>
        Assert.Equal(expected, Explain(degree, Operation.Root, radicand, root, hidden));

    [Theory]
    [InlineData("km", "m", 3, 3000, HiddenPart.Result, "1 km = 1000 m → 3 × 1000 = 3000")]
    [InlineData("km", "m", 3, 3000, HiddenPart.Left, "1 km = 1000 m → 3000 ÷ 1000 = 3")]
    [InlineData("g", "kg", 5000, 5, HiddenPart.Result, "1 kg = 1000 g → 5000 ÷ 1000 = 5")]
    [InlineData("dL", "L", 40, 4, HiddenPart.Left, "1 L = 10 dL → 4 × 10 = 40")]
    public void Unit_conversions_multiply_or_divide_by_the_factor(string from, string to, long left, long result,
        HiddenPart hidden, string expected)
    {
        var (f, t) = (MetricUnits.All.Single(u => u.Symbol == from), MetricUnits.All.Single(u => u.Symbol == to));
        var factor = f.Exponent > t.Exponent ? MetricUnits.Factor(f, t) : MetricUnits.Factor(t, f);
        Assert.Equal(expected, Explainer.Explain(new MathTask(left, Operation.Convert, factor, result, hidden, FromUnit: f, ToUnit: t)));
    }

    [Fact]
    public void Missing_operator_shows_the_whole_equation() =>
        Assert.Equal("6 × 3 = 18", Explain(6, Operation.Multiply, 3, 18, HiddenPart.Operator));

    [Fact]
    public void Fractions_with_different_denominators_use_a_common_one()
    {
        // 1/2 + 1/4 over 4; 1 1/2 + 3/4 over 4.
        Assert.Equal("1/2 + 1/4 = 2/4 + 1/4 = 3/4",
            Explainer.Explain(new MathTask(2, Operation.Add, 1, 3, HiddenPart.Result, NumberKind.Fraction, 4)));
        Assert.Equal("1 1/2 + 3/4 = 6/4 + 3/4 = 9/4 = 2 1/4",
            Explainer.Explain(new MathTask(6, Operation.Add, 3, 9, HiddenPart.Result, NumberKind.Fraction, 4)));
        Assert.Null(Explainer.Explain(new MathTask(1, Operation.Add, 3, 4, HiddenPart.Result, NumberKind.Fraction, 4)));
    }
}
