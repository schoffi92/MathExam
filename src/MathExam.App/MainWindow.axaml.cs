using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MathExam.App.ViewModels;

namespace MathExam.App;

public partial class MainWindow : Window, IFileSaver
{
    // Window size at normal text size; it is multiplied by the text scale.
    private const double BaseWidth = 800;
    private const double BaseHeight = 720;
    // The menu has two columns, so narrower windows would cut it off.
    private const double BaseMinWidth = 800;
    private const double BaseMinHeight = 480;

    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        // Created first: it applies the saved language, which the views read while they load.
        _viewModel = new MainViewModel(this);
        InitializeComponent();
        DataContext = _viewModel;
        FitToTextScale();
        _viewModel.Display.PropertyChanged += OnDisplayChanged;
    }

    private void OnDisplayChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DisplayViewModel.TextScale))
            FitToTextScale();
        else if (e.PropertyName == nameof(DisplayViewModel.Language))
            // Posted, so the language list is not replaced while it is still handling the selection.
            Dispatcher.UIThread.Post(RebuildScreen);
    }

    /// <summary>
    /// Views read their texts once, when they are created ({x:Static}), so after a language change
    /// the screen is replaced by a fresh one. The view models, and so all entered settings, are kept.
    /// </summary>
    private void RebuildScreen()
    {
        Scaler.Child = new ContentControl
        {
            Margin = new Thickness(24),
            [!ContentControl.ContentProperty] = new Binding(nameof(MainViewModel.CurrentViewModel)),
        };
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

    public async Task<SaveTarget?> PickAsync(string title, string suggestedName, string fileTypeName, string extension)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType(fileTypeName) { Patterns = [$"*.{extension}"] }],
        });
        if (file is null)
            return null;

        var stream = await file.OpenWriteAsync();
        // Replacing an existing file: drop its old content beyond what is written now.
        if (stream.CanSeek)
            stream.SetLength(0);
        return new SaveTarget(stream, file.TryGetLocalPath() ?? file.Name);
    }
}
