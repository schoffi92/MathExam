using MathExam.Core;

namespace MathExam.Core.Tests;

public class GameSettingsTests
{
    [Fact]
    public void Valid_settings_have_no_error() =>
        Assert.Null(new GameSettings(1, 10, [Operation.Add]).Validate());

    [Fact]
    public void Min_greater_than_max_is_invalid() =>
        Assert.NotNull(new GameSettings(10, 1, [Operation.Add]).Validate());

    [Fact]
    public void No_operations_is_invalid() =>
        Assert.NotNull(new GameSettings(1, 10, []).Validate());

    [Fact]
    public void Division_with_only_zero_is_invalid() =>
        Assert.NotNull(new GameSettings(0, 0, [Operation.Divide]).Validate());

    [Theory]
    [InlineData(1001, 5000, false)]
    [InlineData(-5000, -1001, false)]
    [InlineData(1000, 5000, true)]
    [InlineData(-5000, 5000, true)]
    public void Powers_and_roots_need_a_number_within_the_base_limit(int min, int max, bool valid)
    {
        Assert.Equal(valid, new GameSettings(min, max, [Operation.Power]).IsValid);
        Assert.Equal(valid, new GameSettings(min, max, [Operation.Root]).IsValid);
        Assert.True(new GameSettings(min, max, [Operation.Add]).IsValid);
    }
}
