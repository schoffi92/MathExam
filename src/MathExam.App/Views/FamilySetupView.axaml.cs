using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace MathExam.App.Views;

public partial class FamilySetupView : UserControl
{
    public FamilySetupView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // Start typing the first name right away.
        this.GetVisualDescendants().OfType<TextBox>().FirstOrDefault()?.Focus();
    }
}
