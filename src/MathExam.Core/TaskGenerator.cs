namespace MathExam.Core;

public sealed class TaskGenerator
{
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
            default:
                throw new InvalidOperationException($"Unknown operation {op}.");
        }

        var hidden = HiddenParts[_random.Next(HiddenParts.Length)];
        if (IsAmbiguous(op, left, right, hidden))
            hidden = HiddenPart.Result;

        return new MathTask(left, op, right, result, hidden);
    }

    private long Pick(GameSettings settings) => _random.NextInt64(settings.Min, (long)settings.Max + 1);

    // "0 × ? = 0" or "0 ÷ ? = 0" accept any answer, so such operands are never hidden.
    private static bool IsAmbiguous(Operation op, long left, long right, HiddenPart hidden) => (op, hidden) switch
    {
        (Operation.Multiply, HiddenPart.Left) => right == 0,
        (Operation.Multiply, HiddenPart.Right) => left == 0,
        (Operation.Divide, HiddenPart.Right) => left == 0,
        _ => false,
    };
}
