using System.Globalization;
using MathExam.App.Resources;

namespace MathExam.App.ViewModels;

public enum AnswerState
{
    Answering,
    Correct,
    Wrong,
}

/// <summary>Answer parsing and feedback texts shared by the solo and family game screens.</summary>
public static class Answers
{
    public static bool TryParse(string text, out long answer) =>
        // Accept the typographic minus as well as the ASCII one.
        long.TryParse(text.Trim().Replace('−', '-'), NumberStyles.Integer, CultureInfo.CurrentCulture, out answer);

    // Spelled out with a symbol so the result is clear without relying on colour.
    public static string Feedback(AnswerState state, long correctAnswer) => state switch
    {
        AnswerState.Correct => Strings.Answer_Correct,
        AnswerState.Wrong => string.Format(Strings.Answer_Wrong, correctAnswer),
        _ => "",
    };

    public static string LevelChange(int change) => change switch
    {
        > 0 => Strings.Answer_LevelUp,
        < 0 => Strings.Answer_LevelDown,
        _ => "",
    };
}
