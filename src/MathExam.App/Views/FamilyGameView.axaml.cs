using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MathExam.App.Views;

public partial class FamilyGameView : UserControl
{
    public FamilyGameView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AnswerBox.Focus();
    }
}
