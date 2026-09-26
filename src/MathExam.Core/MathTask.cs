namespace MathExam.Core;

/// <summary>A piece of an equation's text; exponents and root degrees are superscript.</summary>
public sealed record EquationPart(string Text, bool IsSuperscript = false);

/// <summary>
/// An equation "Left Op Right = Result" with one part hidden. For <see cref="Operation.Power"/>
/// Left is the base and Right the exponent (Left^Right); for <see cref="Operation.Root"/> Left is
/// the degree and Right the radicand (the Left-th root of Right).
/// Decimal and fraction tasks store each number as a numerator over the shared <see cref="Denominator"/>.
/// A <see cref="Operation.Convert"/> task reads "Left FromUnit = Result ToUnit" (e.g. 3 km = 3000 m); its
/// Right is the conversion factor.
/// </summary>
public sealed record MathTask(
    long Left, Operation Op, long Right, long Result, HiddenPart Hidden,
    NumberKind Numbers = NumberKind.Whole, long Denominator = 1,
    MetricUnit? FromUnit = null, MetricUnit? ToUnit = null)
{
    /// <summary>The hidden number (as a numerator over <see cref="Denominator"/>); meaningless for a hidden operator.</summary>
    public long Answer => Hidden switch
    {
        HiddenPart.Left => Left,
        HiddenPart.Right => Right,
        _ => Result,
    };

    /// <summary>The answer as shown to the player: a number, or the operator's symbol.</summary>
    public string AnswerText => Hidden == HiddenPart.Operator ? Op.Symbol() : FormatValue(Answer);

    public bool IsCorrect(long answer) => IsCorrect(new Rational(answer));

    /// <summary>Any equal value counts, so 2/4 and 0.5 are both right for 1/2.</summary>
    public bool IsCorrect(Rational answer)
    {
        if (Hidden == HiddenPart.Operator)
            return false;
        if ((Int128)answer.Numerator * Denominator == (Int128)Answer * answer.Denominator)
            return true;
        // "?² = 9": both 3 and −3 are right.
        return Op == Operation.Power && Hidden == HiddenPart.Left && Right % 2 == 0
               && answer.IsWhole && answer.Numerator == -Left;
    }

    /// <summary>For a hidden operator: any of + − × ÷ that makes the equation true counts, e.g. + and × for 2 ? 2 = 4.</summary>
    public bool IsCorrect(Operation answer) => Hidden == HiddenPart.Operator && Holds(answer);

    /// <summary>Whether "Left op Right = Result" is true for one of the four basic operations.</summary>
    public bool Holds(Operation op)
    {
        Int128 l = Left, r = Right, res = Result, d = Denominator;
        return op switch
        {
            Operation.Add => l + r == res,
            Operation.Subtract => l - r == res,
            // (l/d)(r/d) = res/d  <=>  l·r = res·d
            Operation.Multiply => l * r == res * d,
            // (l/d)/(r/d) = res/d  <=>  l·d = res·r
            Operation.Divide => r != 0 && l * d == res * r,
            _ => false,
        };
    }

    /// <summary>A number of this task (a numerator over <see cref="Denominator"/>) in the task's notation.</summary>
    public string FormatValue(long numerator) => Numbers switch
    {
        NumberKind.Decimal => new Rational(numerator, Denominator).ToDecimalString(),
        NumberKind.Fraction => new Rational(numerator, Denominator).ToFractionString(),
        _ => numerator.ToString(),
    };

    /// <summary>
    /// The equation as text pieces, e.g. "(−3)" "²" " = ?" or "³" "√27 = ?".
    /// With <paramref name="revealAnswer"/> the hidden part is filled in, as on a worksheet's answer key.
    /// </summary>
    public IReadOnlyList<EquationPart> ToParts(bool revealAnswer = false)
    {
        string Part(HiddenPart part, long value) => part == Hidden && !revealAnswer ? "?" : FormatValue(value);
        var equals = $" = {Part(HiddenPart.Result, Result)}";

        switch (Op)
        {
            case Operation.Power:
                var @base = Part(HiddenPart.Left, Left);
                if ((Hidden != HiddenPart.Left || revealAnswer) && Left < 0)
                    @base = $"({@base})";
                return [new(@base), new(Part(HiddenPart.Right, Right), IsSuperscript: true), new(equals)];
            case Operation.Convert:
                return [new($"{Part(HiddenPart.Left, Left)} {FromUnit} = {Part(HiddenPart.Result, Result)} {ToUnit}")];
            case Operation.Root:
                var radical = new EquationPart($"√{Part(HiddenPart.Right, Right)}{equals}");
                // Square roots are written without their degree, unless the degree is the question.
                return Left == 2 && (Hidden != HiddenPart.Left || revealAnswer)
                    ? [radical]
                    : [new(Part(HiddenPart.Left, Left), IsSuperscript: true), radical];
            default:
                var symbol = Hidden == HiddenPart.Operator && !revealAnswer ? "?" : Op.Symbol();
                return [new($"{Part(HiddenPart.Left, Left)} {symbol} {Part(HiddenPart.Right, Right)}{equals}")];
        }
    }

    /// <summary>Plain-text form: superscripts become Unicode superscript digits, or "^" before a hidden exponent.</summary>
    public string ToDisplayString(bool revealAnswer = false) => string.Concat(ToParts(revealAnswer).Select(p =>
        !p.IsSuperscript ? p.Text : ToSuperscript(p.Text) ?? (Op == Operation.Power ? "^" + p.Text : p.Text)));

    public override string ToString() => ToDisplayString();

    /// <summary>The text in Unicode superscript characters, or null if one has none (such as "?").</summary>
    public static string? ToSuperscript(string text)
    {
        const string digits = "0123456789-";
        const string superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻";
        return text.All(digits.Contains) ? string.Concat(text.Select(c => superscripts[digits.IndexOf(c)])) : null;
    }
}
