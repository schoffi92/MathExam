using MathExam.App.Resources;
using MathExam.Core;

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
    /// <summary>
    /// Reads the typed answer for <paramref name="task"/> and submits it: an operator (+ − × ÷) for a missing-operator
    /// task, otherwise a number, which may be a decimal or fraction. Returns whether it was right, or null when the
    /// text is not an answer at all (it is then ignored rather than counted as wrong).
    /// </summary>
    public static bool? Submit(string text, MathTask task, Func<Rational, bool> submitNumber, Func<Operation, bool> submitOperator)
    {
        if (task.Hidden == HiddenPart.Operator)
            return OperationExtensions.TryParseOperator(text, out var op) ? submitOperator(op) : null;
        return Rational.TryParse(text, out var number) ? submitNumber(number) : null;
    }

    // Spelled out with a symbol so the result is clear without relying on colour.
    public static string Feedback(AnswerState state, MathTask task) => state switch
    {
        AnswerState.Correct => Strings.Answer_Correct,
        AnswerState.Wrong => string.Format(Strings.Answer_Wrong, task.AnswerText),
        _ => "",
    };

    /// <summary>After a wrong answer: how to work it out, when there is a simpler way to show.</summary>
    public static string Explanation(AnswerState state, MathTask task) =>
        state == AnswerState.Wrong && Explainer.Explain(task) is { } how ? string.Format(Strings.Answer_HowTo, how) : "";

    public static string LevelChange(int change) => change switch
    {
        > 0 => Strings.Answer_LevelUp,
        < 0 => Strings.Answer_LevelDown,
        _ => "",
    };

    /// <summary>The operator buttons of a missing-operator task.</summary>
    public static IReadOnlyList<string> OperatorSymbols { get; } = OperationExtensions.Basic.Select(o => o.Symbol()).ToList();
}
