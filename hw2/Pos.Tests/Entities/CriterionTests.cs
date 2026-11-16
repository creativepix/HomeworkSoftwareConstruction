using Pos.Domain;

namespace Pos.Tests;

public class CriterionTests
{
    [Fact]
    public void CriterionKeepsValidValues()
    {
        var id = new CriterionId(Guid.NewGuid());

        var criterion = new Criterion(id, "Архитектура", 20m, 2m);

        Assert.Equal(id, criterion.Id);
        Assert.Equal("Архитектура", criterion.Name);
        Assert.Equal(20m, criterion.MaxScore);
        Assert.Equal(2m, criterion.Weight);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CriterionRejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Criterion(new CriterionId(Guid.NewGuid()), name, 10m, 1m));
    }

    [Fact]
    public void CriterionRejectsNullName()
    {
        Assert.Throws<ArgumentException>(() =>
            new Criterion(new CriterionId(Guid.NewGuid()), null!, 10m, 1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriterionRejectsNonPositiveMaxScore(int maxScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Criterion(new CriterionId(Guid.NewGuid()), "Тесты", maxScore, 1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriterionRejectsNonPositiveWeight(int weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Criterion(new CriterionId(Guid.NewGuid()), "Тесты", 10m, weight));
    }
}
