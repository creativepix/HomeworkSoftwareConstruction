namespace Pos.Domain;

public interface IRubricRepository
{
    Rubric? Get();
    Criterion? GetCriterion(CriterionId id);
    IReadOnlyList<Criterion> GetCriteria();
    void Save(Rubric rubric);
    void SaveCriterion(Criterion criterion);
}
