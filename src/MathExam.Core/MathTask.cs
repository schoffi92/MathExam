namespace MathExam.Core;

/// <summary>An equation "Left Op Right = Result" with one number hidden.</summary>
public sealed record MathTask(long Left, Operation Op, long Right, long Result, HiddenPart Hidden)
{
    public long Answer => Hidden switch
    {
        HiddenPart.Left => Left,
        HiddenPart.Right => Right,
        _ => Result,
    };

    public bool IsCorrect(long answer) => answer == Answer;

    public string ToDisplayString()
    {
        string Part(HiddenPart part, long value) => part == Hidden ? "?" : value.ToString();
        return $"{Part(HiddenPart.Left, Left)} {Op.Symbol()} {Part(HiddenPart.Right, Right)} = {Part(HiddenPart.Result, Result)}";
    }

    public override string ToString() => ToDisplayString();
}
