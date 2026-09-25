using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MathExam.App.ViewModels;

namespace MathExam.App;

public partial class MainWindow : Window
{
    // Window size at normal text size; it is multiplied by the text scale.
    private const double BaseWidth = 760;
    private const double BaseHeight = 660;
    private const double BaseMinWidth = 560;
    private const double BaseMinHeight = 480;

    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        FitToTextScale();
        _viewModel.Display.PropertyChanged += OnDisplayChanged;
    }

    private void OnDisplayChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DisplayViewModel.TextScale))
            FitToTextScale();
    }

    /// <summary>Scales the content and resizes the window to match, keeping it inside the screen's work area.</summary>
    private void FitToTextScale()
    {
        var scale = _viewModel.Display.TextScale;
        Scaler.LayoutTransform = new ScaleTransform(scale, scale);

        var area = WorkingAreaSize();
        MinWidth = Math.Min(BaseMinWidth * scale, area.Width);
        MinHeight = Math.Min(BaseMinHeight * scale, area.Height);
        if (WindowState != WindowState.Normal)
            return;

        Width = Math.Min(BaseWidth * scale, area.Width);
        Height = Math.Min(BaseHeight * scale, area.Height);
    }

    /// <summary>Usable screen size in device-independent pixels (unbounded if the screen is unknown).</summary>
    private Size WorkingAreaSize()
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        return screen is null
            ? new Size(double.PositiveInfinity, double.PositiveInfinity)
            : screen.WorkingArea.Size.ToSize(screen.Scaling);
    }
}
