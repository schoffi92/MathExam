using MathExam.Core;

namespace MathExam.Core.Tests;

public sealed class PreferencesStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "MathExamTests", Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_dir, "preferences.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Missing_file_returns_null() =>
        Assert.Null(new PreferencesStore(FilePath).Load());

    [Fact]
    public void Preferences_round_trip()
    {
        var store = new PreferencesStore(FilePath);
        store.Save(new DisplayPreferences(TextSize.ExtraLarge, HighContrast: true));
        Assert.Equal(new DisplayPreferences(TextSize.ExtraLarge, true), new PreferencesStore(FilePath).Load());

        store.Save(new DisplayPreferences());
        Assert.Equal(new DisplayPreferences(TextSize.Normal, false), store.Load());
    }

    [Fact]
    public void Corrupt_file_returns_null()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "not json");
        Assert.Null(new PreferencesStore(FilePath).Load());
    }
}
