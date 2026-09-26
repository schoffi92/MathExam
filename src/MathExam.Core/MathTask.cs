namespace MathExam.Core;

/// <summary>A piece of an equation's text; exponents and root degrees are superscript.</summary>
public sealed record EquationPart(string Text, bool IsSuperscript = false);

/// <summary>
/// An equation "Left Op Right = Result" with one number hidden. For <see cref="Operation.Power"/>
/// Left is the base and Right the exponent (Left^Right); for <see cref="Operation.Root"/> Left is
/// the degree and Right the radicand (the Left-th root of Right).
/// </summary>
public sealed record MathTask(long Left, Operation Op, long Right, long Result, HiddenPart Hidden)
{
    public long Answer => Hidden switch
    {
        HiddenPart.Left => Left,
        HiddenPart.Right => Right,
        _ => Result,
    };

    // "?² = 9": both 3 and −3 are right.
    public bool IsCorrect(long answer) =>
        answer == Answer || (Op == Operation.Power && Hidden == HiddenPart.Left && Right % 2 == 0 && answer == -Left);

    /// <summary>The equation as text pieces, e.g. "(−3)" "²" " = ?" or "³" "√27 = ?".</summary>
    public IReadOnlyList<EquationPart> ToParts()
    {
        string Part(HiddenPart part, long value) => part == Hidden ? "?" : value.ToString();
        var equals = $" = {Part(HiddenPart.Result, Result)}";

        switch (Op)
        {
            case Operation.Power:
                var @base = Part(HiddenPart.Left, Left);
                if (Hidden != HiddenPart.Left && Left < 0)
                    @base = $"({@base})";
                return [new(@base), new(Part(HiddenPart.Right, Right), IsSuperscript: true), new(equals)];
            case Operation.Root:
                var radical = new EquationPart($"√{Part(HiddenPart.Right, Right)}{equals}");
                // Square roots are written without their degree, unless the degree is the question.
                return Left == 2 && Hidden != HiddenPart.Left
                    ? [radical]
                    : [new(Part(HiddenPart.Left, Left), IsSuperscript: true), radical];
            default:
                return [new($"{Part(HiddenPart.Left, Left)} {Op.Symbol()} {Part(HiddenPart.Right, Right)}{equals}")];
        }
    }

    /// <summary>Plain-text form: superscripts become Unicode superscript digits, or "^" before a hidden exponent.</summary>
    public string ToDisplayString() => string.Concat(ToParts().Select(p =>
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
