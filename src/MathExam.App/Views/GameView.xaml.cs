using System.Windows;
using System.Windows.Controls;

namespace MathExam.App.Views;

public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => AnswerBox.Focus();
}
