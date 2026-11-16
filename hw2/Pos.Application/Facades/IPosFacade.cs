namespace Pos.Application;

public interface IPosFacade
{
    StudentData AddStudent(string name);
    IReadOnlyList<StudentData> GetStudents();
    CriterionData AddCriterion(string name, decimal maxScore, decimal weight);
    IReadOnlyList<CriterionData> GetCriteria();
    EvaluationData SetEvaluation(
        Guid studentId,
        Guid criterionId,
        decimal score,
        string? comment = null);
    EvaluationData? GetEvaluation(Guid studentId, Guid criterionId);
    bool DeleteEvaluation(Guid studentId, Guid criterionId);
    decimal GetFinalScore(Guid studentId);
    void ExportResults(string path);
}
