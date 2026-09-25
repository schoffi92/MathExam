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

    /// <summary>Returns a user-facing error message, or null when the settings are valid.</summary>
    public string? Validate()
    {
        if (Min > Max)
            return "The lowest number must not be greater than the highest number.";
        if (Operations.Count == 0)
            return "Select at least one operation.";
        if (Operations.Contains(Operation.Divide) && Min == 0 && Max == 0)
            return "Division needs a non-zero number in the range.";
        return null;
    }

    public bool IsValid => Validate() is null;
}
