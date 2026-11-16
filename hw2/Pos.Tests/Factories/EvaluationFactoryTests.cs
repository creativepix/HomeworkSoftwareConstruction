using Pos.Domain;

namespace Pos.Tests;

public class EvaluationFactoryTests
{
    [Fact]
    public void FactoryCreatesEvaluationForExistingCriterion()
    {
        var rubric = new Rubric();
        var criterion = new Criterion(new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m);
        rubric.AddCriterion(criterion);
        var factory = new EvaluationFactory(rubric);
        var evaluationId = new EvaluationId(Guid.NewGuid());
        var studentId = new StudentId(Guid.NewGuid());
        var date = new DateTime(2026, 11, 16);

        var evaluation = factory.Create(
            evaluationId, studentId, criterion.Id, 8m, date, "Хорошая работа");

        Assert.Equal(evaluationId, evaluation.Id);
        Assert.Equal(studentId, evaluation.StudentId);
        Assert.Equal(criterion.Id, evaluation.CriterionId);
        Assert.Equal(8m, evaluation.Score.Value);
        Assert.Equal(date, evaluation.Date);
        Assert.Equal("Хорошая работа", evaluation.Comment);
    }

    [Fact]
    public void FactoryRejectsUnknownCriterion()
    {
        var factory = new EvaluationFactory(new Rubric());

        Assert.Throws<KeyNotFoundException>(() => factory.Create(
            new EvaluationId(Guid.NewGuid()),
            new StudentId(Guid.NewGuid()),
            new CriterionId(Guid.NewGuid()),
            5m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void FactoryRejectsInvalidScore(int score)
    {
        var rubric = new Rubric();
        var criterion = new Criterion(new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m);
        rubric.AddCriterion(criterion);
        var factory = new EvaluationFactory(rubric);

        Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create(
            new EvaluationId(Guid.NewGuid()),
            new StudentId(Guid.NewGuid()),
            criterion.Id,
            score));
    }
}
