using System.Text.Json;
using System.Text.Json.Serialization;

namespace MathExam.Core;

/// <summary>Shared JSON settings and file helpers for the app's data files.</summary>
internal static class JsonFile
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>The folder holding the app's data files: %LOCALAPPDATA%\MathExam.</summary>
    public static string AppDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathExam");

    public static T? Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);

    /// <summary>Writes to a temp file first so a crash mid-write cannot corrupt the existing file.</summary>
    public static void WriteAtomic<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(value, Options));
        File.Move(tempPath, path, overwrite: true);
    }
}
