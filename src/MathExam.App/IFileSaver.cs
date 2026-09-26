namespace MathExam.App;

/// <summary>A file the user chose to save to.</summary>
/// <param name="Stream">Write the content here, then dispose it.</param>
/// <param name="DisplayName">The file's path (or name, if it has no local path) to show in messages.</param>
public sealed record SaveTarget(Stream Stream, string DisplayName);

/// <summary>Asks the user where to save a file. Implemented by the main window with the system save dialog.</summary>
public interface IFileSaver
{
    /// <param name="fileTypeName">Shown in the dialog's file type list, e.g. "PDF".</param>
    /// <param name="extension">Without the dot, e.g. "pdf".</param>
    /// <returns>The chosen file, or null when the user cancelled.</returns>
    Task<SaveTarget?> PickAsync(string title, string suggestedName, string fileTypeName, string extension);
}
