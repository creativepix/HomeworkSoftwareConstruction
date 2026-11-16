namespace Pos.Domain;

public class EvaluationFactory
{
    private readonly Rubric _rubric;

    public EvaluationFactory(Rubric rubric)
    {
        _rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
    }

    public Evaluation Create(
        EvaluationId id,
        StudentId studentId,
        CriterionId criterionId,
        decimal score,
        DateTime? date = null,
        string? comment = null)
    {
        var criterion = _rubric.GetCriterion(criterionId);
        var checkedScore = new Score(score, criterion.MaxScore);

        return new Evaluation(id, studentId, criterionId, checkedScore, date, comment);
    }
}
