using MathExam.Core.Resources;

namespace MathExam.Core;

public sealed class GameSettings
{
    public GameSettings(int min, int max, IEnumerable<Operation> operations)
    {
        Min = min;
        Max = max;
        Operations = new HashSet<Operation>(operations);
    }

    public int Min { get; }
    public int Max { get; }
    public IReadOnlySet<Operation> Operations { get; }

    /// <summary>Returns a user-facing error message in the current UI language, or null when the settings are valid.</summary>
    public string? Validate()
    {
        if (Min > Max)
            return Strings.Settings_MinGreaterThanMax;
        if (Operations.Count == 0)
            return Strings.Settings_NoOperation;
        if (Operations.Contains(Operation.Divide) && Min == 0 && Max == 0)
            return Strings.Settings_DivisionNeedsNonZero;
        return null;
    }

    public bool IsValid => Validate() is null;
}
