using System.Globalization;
using MathExam.Core;

namespace MathExam.Core.Tests;

public class RationalTests
{
    [Theory]
    [InlineData("12", 12, 1)]
    [InlineData(" -3 ", -3, 1)]
    [InlineData("−3", -3, 1)]
    [InlineData("+4", 4, 1)]
    [InlineData("3/4", 3, 4)]
    [InlineData("6/8", 3, 4)]
    [InlineData("1 1/2", 3, 2)]
    [InlineData("-1 1/2", -3, 2)]
    [InlineData("0.75", 3, 4)]
    [InlineData("0,75", 3, 4)]
    [InlineData("1.5", 3, 2)]
    [InlineData("-0.5", -1, 2)]
    [InlineData("4/2", 2, 1)]
    public void Parses_whole_numbers_decimals_fractions_and_mixed_numbers(string text, long numerator, long denominator)
    {
        Assert.True(Rational.TryParse(text, out var value));
        Assert.Equal(new Rational(numerator, denominator), value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1/0")]
    [InlineData("1//2")]
    [InlineData("1.2.3")]
    [InlineData("--1")]
    [InlineData("1 1")]
    [InlineData("99999999999999999999")]
    [InlineData("1.")]
    public void Rejects_invalid_text(string text) => Assert.False(Rational.TryParse(text, out _));

    [Theory]
    [InlineData(6, 8, "3/4")]
    [InlineData(-7, 4, "-1 3/4")]
    [InlineData(9, 4, "2 1/4")]
    [InlineData(-1, 2, "-1/2")]
    [InlineData(8, 4, "2")]
    [InlineData(0, 5, "0")]
    public void Formats_fractions_reduced_and_as_mixed_numbers(long n, long d, string text) =>
        Assert.Equal(text, new Rational(n, d).ToFractionString());

    [Fact]
    public void Formats_decimals_in_the_current_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Assert.Equal("1.7", new Rational(17, 10).ToDecimalString());
            CultureInfo.CurrentCulture = new CultureInfo("hu-HU");
            Assert.Equal("1,7", new Rational(17, 10).ToDecimalString());
            Assert.Equal("3", new Rational(30, 10).ToDecimalString());
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Is_stored_reduced_with_a_positive_denominator()
    {
        var r = new Rational(4, -6);
        Assert.Equal((-2, 3), (r.Numerator, r.Denominator));
        Assert.Equal(new Rational(1, 2), new Rational(2, 4));
    }
}
