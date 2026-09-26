namespace MathExam.Core;

public sealed class TaskGenerator
{
    /// <summary>Highest exponent of a power, and highest degree of a root.</summary>
    public const int MaxExponent = 5;

    /// <summary>Exponents above 2 are only used while the power stays within this (e.g. 10³, 2⁵).</summary>
    public const long MaxHigherPower = 1000;

    /// <summary>Denominators of fraction tasks; two operands' denominators have a common multiple of at most 24.</summary>
    public static readonly int[] FractionDenominators = [2, 3, 4, 5, 6, 8, 10, 12];

    /// <summary>Decimal tasks use tenths.</summary>
    public const int DecimalDenominator = 10;

    private const int MaxCommonDenominator = 24;

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

        if (settings.Numbers != NumberKind.Whole)
            return NextFractional(settings, op);

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

        return new MathTask(left, op, right, result, PickHidden(settings, op, left, right));
    }

    /// <summary>
    /// Addition or subtraction of tenths or fractions within the range. Fraction operands may have different
    /// denominators; all numbers are stored over their common denominator.
    /// </summary>
    private MathTask NextFractional(GameSettings settings, Operation op)
    {
        long d1, d2;
        if (settings.Numbers == NumberKind.Decimal)
        {
            d1 = d2 = DecimalDenominator;
        }
        else
        {
            d1 = FractionDenominators[_random.Next(FractionDenominators.Length)];
            var partners = FractionDenominators.Where(d => Rational.Lcm(d1, d) <= MaxCommonDenominator).ToArray();
            // Half of the tasks keep one denominator, the easier kind.
            d2 = _random.Next(2) == 0 ? d1 : partners[_random.Next(partners.Length)];
        }
        var denominator = Rational.Lcm(d1, d2);

        // At least one operand should not be a whole number, or it is not a fraction task; a range
        // like 0–0 has nothing else, hence the limited attempts.
        long left = 0, right = 0;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            left = PickMultiple(settings, d1) * (denominator / d1);
            right = PickMultiple(settings, d2) * (denominator / d2);
            if (left % denominator != 0 || right % denominator != 0)
                break;
        }

        if (op == Operation.Subtract && settings.Min >= 0 && left < right)
            (left, right) = (right, left);
        var result = op == Operation.Add ? left + right : left - right;
        return new MathTask(left, op, right, result, PickHidden(settings, op, left, right), settings.Numbers, denominator);
    }

    private long Pick(GameSettings settings) => _random.NextInt64(settings.Min, (long)settings.Max + 1);

    /// <summary>A multiple of 1/<paramref name="denominator"/> within the range, as a numerator.</summary>
    private long PickMultiple(GameSettings settings, long denominator) =>
        _random.NextInt64(settings.Min * denominator, settings.Max * denominator + 1);

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

    /// <summary>
    /// A random part to hide: a number, or with <see cref="GameSettings.MissingOperator"/> also the operator of
    /// + − × ÷ tasks. A number that would make any answer right is replaced by the result.
    /// </summary>
    private HiddenPart PickHidden(GameSettings settings, Operation op, long left, long right)
    {
        var count = settings.MissingOperator && op.IsBasic() ? 4 : 3;
        var hidden = (HiddenPart)_random.Next(count);
        return IsAmbiguous(op, left, right, hidden) ? HiddenPart.Result : hidden;
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
