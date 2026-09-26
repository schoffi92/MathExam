namespace MathExam.Core;

public enum Operation
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Power,
    Root,
    /// <summary>Metric unit conversion, e.g. 3 km = ? m.</summary>
    Convert,
}

public static class OperationExtensions
{
    /// <summary>The four operations that can be the answer of a missing-operator task.</summary>
    public static readonly Operation[] Basic = [Operation.Add, Operation.Subtract, Operation.Multiply, Operation.Divide];

    public static string Symbol(this Operation op) => op switch
    {
        Operation.Add => "+",
        Operation.Subtract => "−",
        Operation.Multiply => "×",
        Operation.Divide => "÷",
        Operation.Power => "xⁿ",
        Operation.Root => "√",
        Operation.Convert => "↔",
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    /// <summary>Power and root take their base from the range but add an exponent (degree) of their own.</summary>
    public static bool IsPowerOrRoot(this Operation op) => op is Operation.Power or Operation.Root;

    /// <summary>+ − × ÷, the operations that can be hidden in a missing-operator task.</summary>
    public static bool IsBasic(this Operation op) => op is Operation.Add or Operation.Subtract or Operation.Multiply or Operation.Divide;

    /// <summary>Reads a typed operator: + - − * x × · / : ÷ (letters in either case).</summary>
    public static bool TryParseOperator(string? text, out Operation op)
    {
        op = default;
        switch (text?.Trim().ToLowerInvariant())
        {
            case "+": op = Operation.Add; return true;
            case "-" or "−" or "–": op = Operation.Subtract; return true;
            case "*" or "x" or "×" or "·": op = Operation.Multiply; return true;
            case "/" or ":" or "÷": op = Operation.Divide; return true;
            default: return false;
        }
    }
}
