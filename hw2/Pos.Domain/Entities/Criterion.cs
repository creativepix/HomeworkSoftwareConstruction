namespace Pos.Domain;

public class Criterion
{
    public Criterion(CriterionId id, string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Criterion name cannot be empty.", nameof(name));
        }

        if (maxScore <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxScore), "Maximum score must be positive.");
        }

        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        Id = id;
        Name = name;
        MaxScore = maxScore;
        Weight = weight;
    }

    public CriterionId Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }
}
