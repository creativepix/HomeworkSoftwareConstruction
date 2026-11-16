namespace Pos.Domain;

public class Evaluation
{
    internal Evaluation(
        EvaluationId id,
        StudentId studentId,
        CriterionId criterionId,
        Score score,
        DateTime? date,
        string? comment)
    {
        Id = id;
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
        Date = date;
        Comment = comment;
    }

    public EvaluationId Id { get; }
    public StudentId StudentId { get; }
    public CriterionId CriterionId { get; }
    public Score Score { get; }
    public DateTime? Date { get; }
    public string? Comment { get; }
}
