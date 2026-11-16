using Pos.Domain;

namespace Pos.Tests;

public class RubricTests
{
    [Fact]
    public void RubricStoresAddedCriterion()
    {
        var rubric = new Rubric();
        var criterion = CreateCriterion("Архитектура");

        rubric.AddCriterion(criterion);

        Assert.Equal(criterion, rubric.GetCriterion(criterion.Id));
        Assert.Contains(criterion, rubric.Criteria);
    }

    [Fact]
    public void RubricRejectsDuplicateCriterionNameIgnoringCase()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(CreateCriterion("Тесты"));

        Assert.Throws<InvalidOperationException>(() =>
            rubric.AddCriterion(CreateCriterion("тесты")));
    }

    [Fact]
    public void RubricRejectsDuplicateCriterionId()
    {
        var rubric = new Rubric();
        var id = new CriterionId(Guid.NewGuid());
        rubric.AddCriterion(new Criterion(id, "Тесты", 10m, 1m));

        Assert.Throws<InvalidOperationException>(() =>
            rubric.AddCriterion(new Criterion(id, "Архитектура", 10m, 1m)));
    }

    [Fact]
    public void RubricRejectsUnknownCriterion()
    {
        var rubric = new Rubric();

        Assert.Throws<KeyNotFoundException>(() =>
            rubric.GetCriterion(new CriterionId(Guid.NewGuid())));
    }

    private static Criterion CreateCriterion(string name)
    {
        return new Criterion(new CriterionId(Guid.NewGuid()), name, 10m, 1m);
    }
}
