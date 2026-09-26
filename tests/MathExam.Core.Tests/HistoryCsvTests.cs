using MathExam.Core;

namespace MathExam.Core.Tests;

public class HistoryCsvTests
{
    private static string[] CsvLines(params SessionRecord[] records)
    {
        var writer = new StringWriter();
        HistoryCsv.Write(writer, records);
        return writer.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
    }

    [Fact]
    public void Writes_a_header_and_one_row_per_game_in_invariant_format()
    {
        var record = new SessionRecord(new DateTime(2026, 9, 26, 14, 5, 0), TimeSpan.FromSeconds(75.25), 1, 10,
            [Operation.Add, Operation.Divide], 7, 1, 1, 3, "Anna, the \"Great\"",
            new Dictionary<Operation, OperationStats> { [Operation.Add] = new(5, 0), [Operation.Divide] = new(2, 1) },
            60, NumberKind.Whole, MissingOperator: true);

        var lines = CsvLines(record);

        Assert.StartsWith("Started,Player,Mode,Min,Max,Operations,Numbers,MissingOperator,Answered,Correct,Wrong,Accuracy,", lines[0]);
        Assert.EndsWith("RootCorrect,RootWrong", lines[0]);
        Assert.Equal(
            "2026-09-26 14:05:00,\"Anna, the \"\"Great\"\"\",Family,1,10,Add Divide,Whole,true,8,7,1,0.875,75.3,60,1,3," +
            "5,0,0,0,0,0,2,1,0,0,0,0",
            lines[1]);
    }

    [Fact]
    public void Games_without_per_operation_counts_leave_those_columns_empty()
    {
        var old = new SessionRecord(new DateTime(2026, 1, 2), TimeSpan.FromMinutes(1), 1, 10, [Operation.Add], 3, 0, null, null);
        var row = CsvLines(old)[1];
        Assert.EndsWith(",,,,,,,,,,,,", row);
        Assert.Contains(",Practice,1,10,", row);
        Assert.Contains(",3,3,0,1,60,,,", row);
    }
}
