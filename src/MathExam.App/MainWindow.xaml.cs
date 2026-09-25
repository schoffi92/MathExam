using System.Windows;
using MathExam.App.ViewModels;

namespace MathExam.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
