using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MathExam.Core.Tests;

public sealed partial class TranslationTests
{
    public static TheoryData<string> Translations()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot(), "src"), "Strings.*.resx", SearchOption.AllDirectories))
            data.Add(Path.GetRelativePath(RepoRoot(), file));
        return data;
    }

    [Fact]
    public void All_supported_languages_are_translated()
    {
        var files = Translations().Select(row => Path.GetFileName((string)row[0])).ToHashSet();
        foreach (var lang in new[] { "fr", "de", "hu" })
            Assert.Contains($"Strings.{lang}.resx", files);
    }

    /// <summary>Each translation has exactly the English keys, with the same {n} placeholders.</summary>
    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_matches_English(string relativePath)
    {
        var path = Path.Combine(RepoRoot(), relativePath);
        var english = Read(Path.Combine(Path.GetDirectoryName(path)!, "Strings.resx"));
        var translated = Read(path);

        Assert.Equal(english.Keys.Order(), translated.Keys.Order());
        foreach (var (key, text) in english)
        {
            Assert.False(string.IsNullOrWhiteSpace(translated[key]), $"{key} is empty");
            Assert.True(Placeholders(text).SetEquals(Placeholders(translated[key])), $"{key} has different placeholders");
        }
    }

    [Fact]
    public void Messages_follow_the_UI_language()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("hu");
            Assert.Equal("Válassz legalább egy műveletet.", new GameSettings(1, 10, []).Validate());
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            Assert.Equal("Select at least one operation.", new GameSettings(1, 10, []).Validate());
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static Dictionary<string, string> Read(string path) =>
        XDocument.Load(path).Root!.Elements("data")
            .ToDictionary(d => (string)d.Attribute("name")!, d => (string)d.Element("value")!);

    private static HashSet<string> Placeholders(string text) =>
        PlaceholderRegex().Matches(text).Select(m => m.Value).ToHashSet();

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex();

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "MathExam.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("MathExam.sln not found.");
    }
}
