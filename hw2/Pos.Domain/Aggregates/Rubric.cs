namespace Pos.Domain;

public class Rubric
{
    private readonly List<Criterion> _criteria = new List<Criterion>();

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public void AddCriterion(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);

        foreach (var current in _criteria)
        {
            if (current.Id == criterion.Id)
            {
                throw new InvalidOperationException("A criterion with this identifier already exists.");
            }

            if (string.Equals(current.Name, criterion.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A criterion with this name already exists.");
            }
        }

        _criteria.Add(criterion);
    }

    public Criterion GetCriterion(CriterionId criterionId)
    {
        foreach (var criterion in _criteria)
        {
            if (criterion.Id == criterionId)
            {
                return criterion;
            }
        }

        throw new KeyNotFoundException("Criterion is not part of this rubric.");
    }
}
