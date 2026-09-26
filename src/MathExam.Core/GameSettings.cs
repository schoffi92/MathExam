using MathExam.Core.Resources;

namespace MathExam.Core;

public sealed class GameSettings
{
    /// <summary>Powers and roots use only the numbers of the range within ± this limit.</summary>
    public const int PowerBaseLimit = 1000;

    /// <param name="numbers">Whole numbers, or decimals/fractions (addition and subtraction only).</param>
    /// <param name="missingOperator">Whether + − × ÷ tasks may hide the operator instead of a number.</param>
    public GameSettings(int min, int max, IEnumerable<Operation> operations,
        NumberKind numbers = NumberKind.Whole, bool missingOperator = false)
    {
        Min = min;
        Max = max;
        Operations = new HashSet<Operation>(operations);
        Numbers = numbers;
        MissingOperator = missingOperator;
    }

    public int Min { get; }
    public int Max { get; }
    public IReadOnlySet<Operation> Operations { get; }
    public NumberKind Numbers { get; }
    public bool MissingOperator { get; }

    /// <summary>The same settings with another range (used by the adaptive levels).</summary>
    public GameSettings WithRange(int min, int max) => new(min, max, Operations, Numbers, MissingOperator);

    /// <summary>Returns a user-facing error message in the current UI language, or null when the settings are valid.</summary>
    public string? Validate()
    {
        if (Min > Max)
            return Strings.Settings_MinGreaterThanMax;
        if (Operations.Count == 0)
            return Strings.Settings_NoOperation;
        // Decimals also work with unit conversion (2.5 m = 250 cm); fractions only with + and −.
        if (Numbers != NumberKind.Whole && Operations.Any(o => o is not (Operation.Add or Operation.Subtract)
                && !(o == Operation.Convert && Numbers == NumberKind.Decimal)))
            return Strings.Settings_FractionsNeedAddSub;
        if (Operations.Contains(Operation.Convert) && Max < 1)
            return Strings.Settings_UnitsNeedPositive;
        if (Operations.Contains(Operation.Divide) && Min == 0 && Max == 0)
            return Strings.Settings_DivisionNeedsNonZero;
        if (Operations.Any(o => o.IsPowerOrRoot()) && (Max < -PowerBaseLimit || Min > PowerBaseLimit))
            return string.Format(Strings.Settings_PowerRangeTooFar, PowerBaseLimit);
        return null;
    }

    public bool IsValid => Validate() is null;
}
