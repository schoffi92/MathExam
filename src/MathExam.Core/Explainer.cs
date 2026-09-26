namespace MathExam.Core;

/// <summary>
/// Shows how to find a task's answer, as a line of arithmetic that needs no translation, e.g.
/// "8 + 7 = 8 + 2 + 5 = 10 + 5 = 15" or "7 × 8 = 56 → 56 ÷ 8 = 7".
/// </summary>
public static class Explainer
{
    /// <summary>The working for the task, or null when there is nothing simpler to show (e.g. 2 + 3).</summary>
    public static string? Explain(MathTask t)
    {
        if (t.Hidden == HiddenPart.Operator)
            return t.ToDisplayString(revealAnswer: true);

        string V(long n) => t.FormatValue(n);
        // Negative numbers after the first term go in brackets: 5 − (−3).
        string T(long n) => n < 0 ? $"({V(n)})" : V(n);
        var solved = t.ToDisplayString(revealAnswer: true);
        long l = t.Left, r = t.Right, res = t.Result;

        return (t.Op, t.Hidden) switch
        {
            // A hidden operand: undo the operation.
            (Operation.Add, HiddenPart.Left) => $"{V(res)} − {T(r)} = {V(l)}",
            (Operation.Add, HiddenPart.Right) => $"{V(res)} − {T(l)} = {V(r)}",
            (Operation.Subtract, HiddenPart.Left) => $"{V(res)} + {T(r)} = {V(l)}",
            (Operation.Subtract, HiddenPart.Right) => $"{V(l)} − {T(res)} = {V(r)}",
            (Operation.Multiply, HiddenPart.Left) => $"{V(res)} ÷ {T(r)} = {V(l)}",
            (Operation.Multiply, HiddenPart.Right) => $"{V(res)} ÷ {T(l)} = {V(r)}",
            (Operation.Divide, HiddenPart.Left) => $"{V(res)} × {T(r)} = {V(l)}",
            (Operation.Divide, HiddenPart.Right) => $"{V(l)} ÷ {T(res)} = {V(r)}",

            // Unit conversion: from the relation of the two units, multiply or divide by the factor.
            (Operation.Convert, _) => Conversion(t),

            // Division, powers and roots: go back to multiplication.
            (Operation.Divide, _) => $"{V(res)} × {T(r)} = {V(l)} → {solved}",
            (Operation.Power, HiddenPart.Result) => $"{solved[..solved.LastIndexOf(" = ", StringComparison.Ordinal)]} = {Repeat(l, r)} = {V(res)}",
            (Operation.Power, HiddenPart.Left) => $"{Repeat(l, r)} = {V(res)}",
            (Operation.Power, _) => $"{Repeat(l, r)} = {V(res)} → {solved}",
            (Operation.Root, _) => $"{Repeat(res, l)} = {V(r)} → {solved}",

            // A hidden result of + − ×: split the numbers into easier steps.
            _ when t.Numbers == NumberKind.Fraction => CommonDenominator(t),
            _ when t.Numbers != NumberKind.Whole || l < 0 || r < 0 => null,
            (Operation.Add, _) => Add(l, r),
            (Operation.Subtract, _) => Subtract(l, r),
            (Operation.Multiply, _) => Multiply(l, r),
            _ => null,
        };

        string Repeat(long factor, long times) => string.Join(" × ", Enumerable.Repeat(T(factor), (int)times));
    }

    /// <summary>
    /// "1 km = 1000 m → 3 × 1000 = 3000" when converting to the smaller unit, "… → 3000 ÷ 1000 = 3" to the larger.
    /// With the first amount hidden, the steps run the other way.
    /// </summary>
    private static string? Conversion(MathTask t)
    {
        if (t.FromUnit is not { } from || t.ToUnit is not { } to)
            return null;
        var toSmaller = from.Exponent > to.Exponent;
        var (large, small) = toSmaller ? (from, to) : (to, from);
        var relation = $"1 {large} = {t.Right} {small}";

        // Known amount → hidden amount: multiply going to the smaller unit, divide going to the larger.
        var (known, hidden) = t.Hidden == HiddenPart.Left ? (t.Result, t.Left) : (t.Left, t.Result);
        var multiply = toSmaller == (t.Hidden != HiddenPart.Left);
        var step = multiply ? $"{t.FormatValue(known)} × {t.Right}" : $"{t.FormatValue(known)} ÷ {t.Right}";
        return $"{relation} → {step} = {t.FormatValue(hidden)}";
    }

    /// <summary>Bridging through ten (8 + 7 = 8 + 2 + 5) or adding tens and ones separately.</summary>
    private static string? Add(long l, long r)
    {
        var sum = l + r;
        if (l < 10 && r < 10)
        {
            if (sum <= 10)
                return null;
            var (big, small) = l >= r ? (l, r) : (r, l);
            var toTen = 10 - big;
            return $"{big} + {small} = {big} + {toTen} + {small - toTen} = 10 + {small - toTen} = {sum}";
        }

        if (l >= 10 && r >= 10)
        {
            long tens = l - l % 10 + (r - r % 10), ones = l % 10 + r % 10;
            if (ones == 0)
                return null;
            var parts = new[] { l - l % 10, r - r % 10, l % 10, r % 10 }.Where(p => p != 0);
            return $"{l} + {r} = {string.Join(" + ", parts)} = {tens} + {ones} = {sum}";
        }

        {
            var (big, small) = l >= 10 ? (l, r) : (r, l);
            var ones = big % 10;
            if (small == 0 || ones + small == 10)
                return null;
            if (ones + small < 10)
                return $"{big} + {small} = {big - ones} + {ones} + {small} = {big - ones} + {ones + small} = {sum}";
            var toNext = 10 - ones;
            return $"{big} + {small} = {big} + {toNext} + {small - toNext} = {big + toNext} + {small - toNext} = {sum}";
        }
    }

    /// <summary>Taking away tens then ones (52 − 17 = 52 − 10 − 7), or bridging down through ten (52 − 7 = 52 − 2 − 5).</summary>
    private static string? Subtract(long l, long r)
    {
        if (l < r)
            return null;
        var diff = l - r;
        long tens = r - r % 10, ones = r % 10;
        if (tens > 0 && ones > 0)
            return $"{l} − {r} = {l} − {tens} − {ones} = {l - tens} − {ones} = {diff}";
        var down = l % 10;
        if (tens == 0 && l >= 10 && down > 0 && down < ones)
            return $"{l} − {r} = {l} − {down} − {ones - down} = {l - down} − {ones - down} = {diff}";
        return null;
    }

    /// <summary>Splitting a factor: 12 × 7 = 10 × 7 + 2 × 7, or 7 × 8 = 7 × 5 + 7 × 3.</summary>
    private static string? Multiply(long l, long r)
    {
        if (l <= 1 || r <= 1)
            return null;
        var product = l * r;
        var splitLeft = l >= r;
        var split = splitLeft ? l : r;
        var other = splitLeft ? r : l;

        long a, b;
        if (split >= 10)
            (a, b) = (split - split % 10, split % 10);
        else if (split > 5)
            (a, b) = (5, split - 5);
        else
            return null;
        if (b == 0)
            return null;

        string Times(long part) => splitLeft ? $"{part} × {other}" : $"{other} × {part}";
        return $"{l} × {r} = {Times(a)} + {Times(b)} = {a * other} + {b * other} = {product}";
    }

    /// <summary>1/2 + 1/4 = 2/4 + 1/4 = 3/4, shown when the denominators differ.</summary>
    private static string? CommonDenominator(MathTask t)
    {
        if (t.Op is not (Operation.Add or Operation.Subtract))
            return null;
        Rational a = new(t.Left, t.Denominator), b = new(t.Right, t.Denominator);
        if (a.Denominator == b.Denominator)
            return null;

        var common = Rational.Lcm(a.Denominator, b.Denominator);
        long x = a.Numerator * (common / a.Denominator), y = b.Numerator * (common / b.Denominator);
        var n = t.Op == Operation.Add ? x + y : x - y;
        var symbol = t.Op.Symbol();
        string Over(long numerator) => numerator < 0 ? $"({numerator}/{common})" : $"{numerator}/{common}";
        string Term(Rational v) => v.Numerator < 0 ? $"({v.ToFractionString()})" : v.ToFractionString();

        var line = $"{a.ToFractionString()} {symbol} {Term(b)} = {Over(x)} {symbol} {Over(y)} = {n}/{common}";
        var result = new Rational(t.Result, t.Denominator).ToFractionString();
        return result == $"{n}/{common}" ? line : $"{line} = {result}";
    }
}
