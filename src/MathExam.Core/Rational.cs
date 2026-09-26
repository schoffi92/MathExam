using System.Globalization;

namespace MathExam.Core;

/// <summary>What kind of numbers a game uses.</summary>
public enum NumberKind
{
    Whole,
    /// <summary>Tenths, e.g. 1.7.</summary>
    Decimal,
    /// <summary>Fractions and mixed numbers, e.g. 3/4 or 1 1/2.</summary>
    Fraction,
}

/// <summary>An exact fraction, always stored reduced and with a positive denominator.</summary>
public readonly record struct Rational
{
    public Rational(long numerator, long denominator = 1)
    {
        if (denominator == 0)
            throw new DivideByZeroException();
        if (denominator < 0)
            (numerator, denominator) = (-numerator, -denominator);
        var gcd = Gcd(Math.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    public long Numerator { get; }
    public long Denominator { get; }

    public bool IsWhole => Denominator == 1;

    /// <summary>
    /// Reads a typed answer: a whole number ("12", "-3"), a decimal with "." or "," ("0.75", "1,5"),
    /// a fraction ("3/4") or a mixed number ("1 1/2"). The typographic minus "−" is accepted too.
    /// </summary>
    public static bool TryParse(string? text, out Rational value)
    {
        value = default;
        var s = text?.Trim().Replace('−', '-').Replace('⁄', '/') ?? "";
        var negative = s.StartsWith('-');
        if (negative || s.StartsWith('+'))
            s = s[1..].TrimStart();
        if (s.Length == 0)
            return false;

        try
        {
            Rational parsed;
            var space = s.IndexOf(' ');
            if (space > 0)
            {
                // Mixed number: whole part, then a fraction.
                if (!TryDigits(s[..space], out var whole) || !TryFraction(s[(space + 1)..].Trim(), out var fraction))
                    return false;
                parsed = new Rational(checked(whole * fraction.Denominator + fraction.Numerator), fraction.Denominator);
            }
            else if (s.Contains('/'))
            {
                if (!TryFraction(s, out parsed))
                    return false;
            }
            else if (s.IndexOfAny(['.', ',']) is var point and >= 0)
            {
                var digits = s[..point] + s[(point + 1)..];
                var decimals = s.Length - point - 1;
                if (decimals is < 1 or > 9 || !TryDigits(digits, out var n))
                    return false;
                parsed = new Rational(n, (long)Math.Pow(10, decimals));
            }
            else if (TryDigits(s, out var n))
            {
                parsed = new Rational(n);
            }
            else
            {
                return false;
            }

            value = negative ? new Rational(-parsed.Numerator, parsed.Denominator) : parsed;
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    /// <summary>"5", "3/4", "1 1/2" or "-1 1/2".</summary>
    public string ToFractionString()
    {
        if (IsWhole)
            return Numerator.ToString(CultureInfo.CurrentCulture);
        var whole = Numerator / Denominator;
        var rest = Math.Abs(Numerator % Denominator);
        if (whole == 0)
            return $"{Numerator.ToString(CultureInfo.CurrentCulture)}/{Denominator}";
        return $"{whole.ToString(CultureInfo.CurrentCulture)} {rest}/{Denominator}";
    }

    /// <summary>Decimal notation in the current culture, e.g. "1.7" or "1,7". Exact for denominators like 10.</summary>
    public string ToDecimalString() => ((decimal)Numerator / Denominator).ToString("0.##########", CultureInfo.CurrentCulture);

    public override string ToString() => ToFractionString();

    internal static long Gcd(long a, long b)
    {
        while (b != 0)
            (a, b) = (b, a % b);
        return a == 0 ? 1 : a;
    }

    internal static long Lcm(long a, long b) => a / Gcd(a, b) * b;

    private static bool TryDigits(string s, out long value)
    {
        value = 0;
        return s.Length > 0 && s.All(char.IsAsciiDigit)
            && long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryFraction(string s, out Rational value)
    {
        value = default;
        var parts = s.Split('/');
        if (parts.Length != 2 || !TryDigits(parts[0].Trim(), out var n) || !TryDigits(parts[1].Trim(), out var d) || d == 0)
            return false;
        value = new Rational(n, d);
        return true;
    }
}
