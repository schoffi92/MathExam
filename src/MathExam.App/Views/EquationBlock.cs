using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using MathExam.Core;

namespace MathExam.App.Views;

/// <summary>
/// Shows a <see cref="MathTask"/>. Exponents and root degrees use the font's superscript digits; a hidden
/// one ("?") has no superscript character, so it is drawn smaller and raised instead.
/// </summary>
public class EquationBlock : TextBlock
{
    public static readonly StyledProperty<MathTask?> EquationProperty =
        AvaloniaProperty.Register<EquationBlock, MathTask?>(nameof(Equation));

    private const double SuperscriptScale = 0.55;

    public MathTask? Equation
    {
        get => GetValue(EquationProperty);
        set => SetValue(EquationProperty, value);
    }

    // Styled like any TextBlock.
    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == EquationProperty || change.Property == FontSizeProperty)
            Rebuild();
    }

    private void Rebuild()
    {
        var inlines = new InlineCollection();
        foreach (var part in Equation?.ToParts() ?? [])
        {
            if (!part.IsSuperscript)
                inlines.Add(new Run(part.Text));
            else if (MathTask.ToSuperscript(part.Text) is { } superscript)
                inlines.Add(new Run(superscript));
            else
                inlines.Add(new Run(part.Text) { FontSize = FontSize * SuperscriptScale, BaselineAlignment = BaselineAlignment.Superscript });
        }
        Inlines = inlines;
    }
}
