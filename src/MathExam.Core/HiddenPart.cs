namespace MathExam.Core;

/// <summary>Which part of the equation "Left op Right = Result" the player has to find.</summary>
public enum HiddenPart
{
    Left,
    Right,
    Result,
    /// <summary>The operation itself, e.g. "6 ? 3 = 18"; only for + − × ÷ with <see cref="GameSettings.MissingOperator"/>.</summary>
    Operator,
}
