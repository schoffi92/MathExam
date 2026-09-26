using Avalonia.Platform;
using SkiaSharp;

namespace MathExam.App;

/// <summary>The app's bundled Inter font, loaded for drawing outside Avalonia (the worksheet PDF).</summary>
public static class InterFonts
{
    private static readonly Lazy<SKTypeface> LazyRegular = new(() => Load("Inter-Regular.ttf"));
    private static readonly Lazy<SKTypeface> LazySemiBold = new(() => Load("Inter-SemiBold.ttf"));

    public static SKTypeface Regular => LazyRegular.Value;
    public static SKTypeface SemiBold => LazySemiBold.Value;

    private static SKTypeface Load(string file)
    {
        using var asset = AssetLoader.Open(new Uri($"avares://Avalonia.Fonts.Inter/Assets/{file}"));
        // Copied, because SkiaSharp reads the font lazily and needs a stream it can keep.
        var copy = new MemoryStream();
        asset.CopyTo(copy);
        copy.Position = 0;
        return SKTypeface.FromStream(copy);
    }
}
