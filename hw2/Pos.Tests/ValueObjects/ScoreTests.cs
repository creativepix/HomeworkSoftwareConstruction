using Pos.Domain;

namespace Pos.Tests;

public class ScoreTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ScoreAcceptsRangeBoundaries(int value)
    {
        var score = new Score(value, 10m);

        Assert.Equal(value, score.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void ScoreRejectsValueOutsideRange(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Score(value, 10m));
    }
}
