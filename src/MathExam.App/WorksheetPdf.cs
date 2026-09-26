using MathExam.Core;
using SkiaSharp;

namespace MathExam.App;

/// <summary>One version of a worksheet: a label (e.g. "Version B · Sheet code: 4821") and its tasks.</summary>
public sealed record WorksheetSheet(string Label, IReadOnlyList<MathTask> Tasks);

/// <summary>The texts and sheets of a printable worksheet.</summary>
public sealed record WorksheetContent(
    string Title, string Settings, string NameLabel, string DateLabel, string Date, string AnswersTitle,
    IReadOnlyList<WorksheetSheet> Sheets);

/// <summary>
/// Writes a worksheet as an A4 PDF with SkiaSharp (which Avalonia already ships): one page per sheet with the
/// tasks and blanks to fill in, then one answer key per sheet, so the task pages can be printed for a class
/// and the keys kept apart. Exponents and root degrees use superscript digits.
/// </summary>
public static class WorksheetPdf
{
    public const int MaxTasks = 30;

    // A4 in points (1/72 inch).
    private const float PageWidth = 595, PageHeight = 842, Margin = 50;

    public static void Write(Stream output, WorksheetContent content, SKTypeface regular, SKTypeface bold)
    {
        using var document = SKDocument.CreatePdf(output, new SKDocumentPdfMetadata { Title = content.Title, Creator = "MathExam" });
        using var ink = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        using var muted = new SKPaint { Color = new SKColor(0x55, 0x55, 0x55), IsAntialias = true };
        using var line = new SKPaint { Color = new SKColor(0x88, 0x88, 0x88), StrokeWidth = 0.8f, IsAntialias = true };

        foreach (var sheet in content.Sheets)
            WriteTaskPage(document, content, sheet, regular, bold, ink, muted, line);
        foreach (var sheet in content.Sheets)
            WriteAnswerPage(document, content, sheet, regular, bold, ink, muted, line);
        document.Close();
    }

    private static void WriteTaskPage(SKDocument document, WorksheetContent content, WorksheetSheet sheet,
        SKTypeface regular, SKTypeface bold, SKPaint ink, SKPaint muted, SKPaint line)
    {
        // Header, then the tasks in two columns.
        var canvas = document.BeginPage(PageWidth, PageHeight);
        var y = Margin + 20;
        canvas.DrawText(content.Title, Margin, y, new SKFont(bold, 22), ink);
        y += 22;
        canvas.DrawText(content.Settings, Margin, y, new SKFont(regular, 11), muted);
        y += 16;
        canvas.DrawText(sheet.Label, Margin, y, new SKFont(regular, 11), muted);
        y += 30;
        var labelFont = new SKFont(regular, 12);
        canvas.DrawText(content.NameLabel, Margin, y, labelFont, ink);
        var nameStart = Margin + labelFont.MeasureText(content.NameLabel) + 6;
        canvas.DrawLine(nameStart, y + 2, nameStart + 200, y + 2, line);
        var dateX = PageWidth - Margin - 170;
        canvas.DrawText(content.DateLabel, dateX, y, labelFont, ink);
        canvas.DrawText(content.Date, dateX + labelFont.MeasureText(content.DateLabel) + 6, y, labelFont, ink);
        y += 20;
        canvas.DrawLine(Margin, y, PageWidth - Margin, y, line);
        DrawTasks(canvas, sheet.Tasks, reveal: false, y + 10, sheet.Tasks.Count > 20 ? 15 : 18, regular, ink, muted, line);
        document.EndPage();
    }

    private static void WriteAnswerPage(SKDocument document, WorksheetContent content, WorksheetSheet sheet,
        SKTypeface regular, SKTypeface bold, SKPaint ink, SKPaint muted, SKPaint line)
    {
        var canvas = document.BeginPage(PageWidth, PageHeight);
        var y = Margin + 20;
        canvas.DrawText($"{content.Title} – {content.AnswersTitle}", Margin, y, new SKFont(bold, 18), ink);
        y += 16;
        canvas.DrawText(sheet.Label, Margin, y, new SKFont(regular, 11), muted);
        y += 12;
        canvas.DrawLine(Margin, y, PageWidth - Margin, y, line);
        DrawTasks(canvas, sheet.Tasks, reveal: true, y + 10, sheet.Tasks.Count > 20 ? 12 : 14, regular, ink, muted, line);
        document.EndPage();
    }

    private static void DrawTasks(SKCanvas canvas, IReadOnlyList<MathTask> tasks, bool reveal, float top, float size,
        SKTypeface typeface, SKPaint ink, SKPaint muted, SKPaint line)
    {
        // Two columns, filled top to bottom; at least ten rows, so short sheets are not spread thin.
        var rows = Math.Max(10, (tasks.Count + 1) / 2);
        var columnWidth = (PageWidth - 2 * Margin) / 2;
        var rowHeight = (PageHeight - Margin - top) / rows;
        var numberFont = new SKFont(typeface, size * 0.7f);
        for (var i = 0; i < tasks.Count; i++)
        {
            var x = Margin + i / rows * columnWidth;
            var baseline = top + (i % rows) * rowHeight + rowHeight * 0.6f;
            canvas.DrawText($"{i + 1}.", x, baseline, numberFont, muted);
            DrawEquation(canvas, tasks[i], reveal, x + 30, baseline, size, columnWidth - 44, typeface, ink, line);
        }
    }

    private enum Blank { None, Number, Operator, Raised }

    /// <summary>
    /// Draws one equation. Each "?" becomes something to write in: a line for a number, a box for an operator,
    /// and a small raised box for an exponent or root degree. A long equation is scaled down to fit.
    /// </summary>
    private static void DrawEquation(SKCanvas canvas, MathTask task, bool reveal, float x, float baseline, float size,
        float maxWidth, SKTypeface typeface, SKPaint ink, SKPaint line)
    {
        var runs = new List<(string Text, Blank Blank)>();
        var numberBlank = task.Hidden == HiddenPart.Operator ? Blank.Operator : Blank.Number;
        foreach (var part in task.ToParts(reveal))
        {
            if (part.IsSuperscript)
            {
                // Digits have superscript characters; a hidden exponent or degree has none.
                runs.Add(MathTask.ToSuperscript(part.Text) is { } superscript ? (superscript, Blank.None) : ("", Blank.Raised));
                continue;
            }
            var pieces = part.Text.Split('?');
            for (var i = 0; i < pieces.Length; i++)
            {
                if (i > 0)
                    runs.Add(("", numberBlank));
                if (pieces[i].Length > 0)
                    runs.Add((pieces[i], Blank.None));
            }
        }

        var font = new SKFont(typeface, size);
        float Width((string Text, Blank Blank) run, SKFont f) => run.Blank switch
        {
            Blank.Number => f.Size * 2.4f,
            Blank.Operator => f.Size * 1.3f,
            Blank.Raised => f.Size * 0.8f,
            _ => f.MeasureText(run.Text),
        };
        var scale = Math.Min(1f, maxWidth / runs.Sum(r => Width(r, font)));
        font = new SKFont(typeface, size * scale);

        using var box = new SKPaint { Color = line.Color, StrokeWidth = line.StrokeWidth, Style = SKPaintStyle.Stroke, IsAntialias = true };
        foreach (var run in runs)
        {
            var width = Width(run, font);
            var s = font.Size;
            switch (run.Blank)
            {
                case Blank.None:
                    canvas.DrawText(run.Text, x, baseline, font, ink);
                    break;
                case Blank.Number:
                    canvas.DrawLine(x + 2, baseline + 2, x + width - 2, baseline + 2, line);
                    break;
                case Blank.Operator:
                    canvas.DrawRect(x + s * 0.2f, baseline - s * 0.85f, width - s * 0.4f, s * 1.0f, box);
                    break;
                case Blank.Raised:
                    canvas.DrawRect(x + s * 0.1f, baseline - s * 1.05f, width - s * 0.2f, s * 0.6f, box);
                    break;
            }
            x += width;
        }
    }
}
