namespace Pos.Domain;

public interface IEvaluationRepository
{
    Evaluation? Get(StudentId studentId, CriterionId criterionId);
    IReadOnlyList<Evaluation> GetByStudent(StudentId studentId);
    IReadOnlyList<Evaluation> GetAllEvaluations();
    void Save(Evaluation evaluation);
    bool Delete(StudentId studentId, CriterionId criterionId);
}
