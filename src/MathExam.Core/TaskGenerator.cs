namespace MathExam.Core;

public sealed class TaskGenerator
{
    /// <summary>Highest exponent of a power, and highest degree of a root.</summary>
    public const int MaxExponent = 5;

    /// <summary>Exponents above 2 are only used while the power stays within this (e.g. 10³, 2⁵).</summary>
    public const long MaxHigherPower = 1000;

    private static readonly HiddenPart[] HiddenParts = Enum.GetValues<HiddenPart>();
    private readonly Random _random;

    public TaskGenerator(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    public MathTask Next(GameSettings settings)
    {
        var error = settings.Validate();
        if (error is not null)
            throw new ArgumentException(error, nameof(settings));

        var ops = settings.Operations.OrderBy(o => o).ToArray();
        var op = ops[_random.Next(ops.Length)];

        long left, right, result;
        switch (op)
        {
            case Operation.Add:
                left = Pick(settings);
                right = Pick(settings);
                result = left + right;
                break;
            case Operation.Subtract:
                left = Pick(settings);
                right = Pick(settings);
                if (settings.Min >= 0 && left < right)
                    (left, right) = (right, left);
                result = left - right;
                break;
            case Operation.Multiply:
                left = Pick(settings);
                right = Pick(settings);
                result = left * right;
                break;
            case Operation.Divide:
                // Built backwards (dividend = divisor * quotient) so the division is always exact.
                do { right = Pick(settings); } while (right == 0);
                result = Pick(settings);
                left = right * result;
                break;
            case Operation.Power:
                left = PickPowerBase(settings);
                right = PickExponent(left, oddOnly: false);
                result = Pow(left, right);
                break;
            case Operation.Root:
                // Built backwards (radicand = root ^ degree) so the root is always a whole number.
                // Negative numbers only have odd roots, e.g. ³√−8 = −2.
                result = PickPowerBase(settings);
                left = PickExponent(result, oddOnly: result < 0);
                right = Pow(result, left);
                break;
            default:
                throw new InvalidOperationException($"Unknown operation {op}.");
        }

        var hidden = HiddenParts[_random.Next(HiddenParts.Length)];
        if (IsAmbiguous(op, left, right, hidden))
            hidden = HiddenPart.Result;

        return new MathTask(left, op, right, result, hidden);
    }

    private long Pick(GameSettings settings) => _random.NextInt64(settings.Min, (long)settings.Max + 1);

    /// <summary>A number from the range that is also within ±<see cref="GameSettings.PowerBaseLimit"/>.</summary>
    private long PickPowerBase(GameSettings settings) => _random.NextInt64(
        Math.Max(settings.Min, -GameSettings.PowerBaseLimit), Math.Min(settings.Max, GameSettings.PowerBaseLimit) + 1L);

    private long PickExponent(long @base, bool oddOnly)
    {
        var choices = new List<long>();
        for (var e = 2; e <= MaxExponent; e++)
        {
            if (oddOnly && e % 2 == 0)
                continue;
            if (e == 2 || Pow(Math.Abs(@base), e) <= MaxHigherPower)
                choices.Add(e);
        }
        // Squares are always possible; an odd root of a large negative number falls back to a cube.
        return choices.Count == 0 ? 3 : choices[_random.Next(choices.Count)];
    }

    // |base| <= PowerBaseLimit and exponent <= MaxExponent, so this cannot overflow.
    private static long Pow(long @base, long exponent)
    {
        long result = 1;
        for (var i = 0; i < exponent; i++)
            result *= @base;
        return result;
    }

    // "0 × ? = 0", "0 ÷ ? = 0", "1^? = 1" or "?√1 = 1" accept any answer, so such operands are never hidden.
    private static bool IsAmbiguous(Operation op, long left, long right, HiddenPart hidden) => (op, hidden) switch
    {
        (Operation.Multiply, HiddenPart.Left) => right == 0,
        (Operation.Multiply, HiddenPart.Right) => left == 0,
        (Operation.Divide, HiddenPart.Right) => left == 0,
        (Operation.Power, HiddenPart.Right) => Math.Abs(left) <= 1,
        (Operation.Root, HiddenPart.Left) => Math.Abs(right) <= 1,
        _ => false,
    };
}
