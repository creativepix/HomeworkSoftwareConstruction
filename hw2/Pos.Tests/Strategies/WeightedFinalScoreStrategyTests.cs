using Pos.Domain;

namespace Pos.Tests;

public class WeightedFinalScoreStrategyTests
{
    [Fact]
    public void EmptyRubricHasZeroFinalScore()
    {
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        var result = strategy.Calculate(
            new StudentId(Guid.NewGuid()), new Rubric(), Array.Empty<Evaluation>());

        Assert.Equal(0m, result);
    }

    [Fact]
    public void MissingEvaluationContributesZero()
    {
        var studentId = new StudentId(Guid.NewGuid());
        var rubric = CreateRubric(out var first, out _);
        var evaluation = CreateEvaluation(rubric, studentId, first, 10m);
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        var result = strategy.Calculate(studentId, rubric, new[] { evaluation });

        Assert.Equal(2.5m, result);
    }

    [Fact]
    public void StrategyUsesScoresMaximumsAndWeights()
    {
        var studentId = new StudentId(Guid.NewGuid());
        var rubric = CreateRubric(out var first, out var second);
        var evaluations = new[]
        {
            CreateEvaluation(rubric, studentId, first, 10m),
            CreateEvaluation(rubric, studentId, second, 10m)
        };
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        var result = strategy.Calculate(studentId, rubric, evaluations);

        Assert.Equal(6.25m, result);
    }

    [Fact]
    public void MaximumScoresGiveTen()
    {
        var studentId = new StudentId(Guid.NewGuid());
        var rubric = CreateRubric(out var first, out var second);
        var evaluations = new[]
        {
            CreateEvaluation(rubric, studentId, first, first.MaxScore),
            CreateEvaluation(rubric, studentId, second, second.MaxScore)
        };
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        var result = strategy.Calculate(studentId, rubric, evaluations);

        Assert.Equal(10m, result);
    }

    [Fact]
    public void StrategyIgnoresAnotherStudentsEvaluation()
    {
        var studentId = new StudentId(Guid.NewGuid());
        var anotherStudentId = new StudentId(Guid.NewGuid());
        var rubric = CreateRubric(out var first, out _);
        var evaluation = CreateEvaluation(rubric, anotherStudentId, first, 10m);
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        var result = strategy.Calculate(studentId, rubric, new[] { evaluation });

        Assert.Equal(0m, result);
    }

    [Fact]
    public void StrategyRejectsNullRubric()
    {
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.Calculate(
            new StudentId(Guid.NewGuid()), null!, Array.Empty<Evaluation>()));
    }

    [Fact]
    public void StrategyRejectsNullEvaluations()
    {
        IFinalScoreStrategy strategy = new WeightedFinalScoreStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.Calculate(
            new StudentId(Guid.NewGuid()), new Rubric(), null!));
    }

    private static Rubric CreateRubric(out Criterion first, out Criterion second)
    {
        var rubric = new Rubric();
        first = new Criterion(new CriterionId(Guid.NewGuid()), "Архитектура", 10m, 1m);
        second = new Criterion(new CriterionId(Guid.NewGuid()), "Тесты", 20m, 3m);
        rubric.AddCriterion(first);
        rubric.AddCriterion(second);
        return rubric;
    }

    private static Evaluation CreateEvaluation(
        Rubric rubric,
        StudentId studentId,
        Criterion criterion,
        decimal score)
    {
        return new EvaluationFactory(rubric).Create(
            new EvaluationId(Guid.NewGuid()), studentId, criterion.Id, score);
    }
}
