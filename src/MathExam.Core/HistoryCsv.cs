using System.Globalization;
using System.Text;

namespace MathExam.Core;

/// <summary>
/// Writes the history as CSV for spreadsheets: comma-separated with English column names, ISO dates and
/// "." decimals whatever the language, so the file reads the same everywhere.
/// </summary>
public static class HistoryCsv
{
    /// <summary>UTF-8 with a byte order mark, so Excel recognises non-English names.</summary>
    public static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public static void Write(TextWriter writer, IEnumerable<SessionRecord> records)
    {
        var operations = Enum.GetValues<Operation>();
        var header = new List<string>
        {
            "Started", "Player", "Min", "Max", "Operations", "Numbers", "MissingOperator", "Answered", "Correct",
            "Wrong", "Accuracy", "DurationSeconds", "TimeLimitSeconds", "StartLevel", "EndLevel",
        };
        foreach (var op in operations)
            header.AddRange([$"{op}Correct", $"{op}Wrong"]);
        WriteRow(writer, header);

        foreach (var r in records.OrderBy(r => r.StartedAt))
        {
            var row = new List<string>
            {
                r.StartedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                r.Player ?? "",
                Number(r.Min),
                Number(r.Max),
                string.Join(" ", r.Operations),
                r.Numbers.ToString(),
                r.MissingOperator ? "true" : "false",
                Number(r.Answered),
                Number(r.Correct),
                Number(r.Wrong),
                r.Accuracy.ToString("0.###", CultureInfo.InvariantCulture),
                r.Duration.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture),
                Number(r.TimeLimitSeconds),
                Number(r.StartLevel),
                Number(r.EndLevel),
            };
            // Games saved before per-operation counts existed leave these columns empty.
            foreach (var op in operations)
            {
                var stats = r.ByOperation?.GetValueOrDefault(op);
                row.AddRange(r.ByOperation is null ? ["", ""] : [Number(stats?.Correct ?? 0), Number(stats?.Wrong ?? 0)]);
            }
            WriteRow(writer, row);
        }
    }

    private static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";

    private static void WriteRow(TextWriter writer, IEnumerable<string> fields) =>
        writer.Write(string.Join(",", fields.Select(Quote)) + "\r\n");

    // RFC 4180: quote fields containing a comma, quote or line break, doubling inner quotes.
    private static string Quote(string field) =>
        field.IndexOfAny([',', '"', '\r', '\n']) < 0 ? field : "\"" + field.Replace("\"", "\"\"") + "\"";
}
