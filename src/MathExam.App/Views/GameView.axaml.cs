using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MathExam.App.Views;

public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AnswerBox.Focus();
    }
}
