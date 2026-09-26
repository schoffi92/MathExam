namespace MathExam.Core;

public enum Operation
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Power,
    Root,
}

public static class OperationExtensions
{
    public static string Symbol(this Operation op) => op switch
    {
        Operation.Add => "+",
        Operation.Subtract => "−",
        Operation.Multiply => "×",
        Operation.Divide => "÷",
        Operation.Power => "xⁿ",
        Operation.Root => "√",
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    /// <summary>Power and root take their base from the range but add an exponent (degree) of their own.</summary>
    public static bool IsPowerOrRoot(this Operation op) => op is Operation.Power or Operation.Root;
}
