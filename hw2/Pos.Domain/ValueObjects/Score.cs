namespace Pos.Domain;

public readonly record struct Score
{
    public Score(decimal value, decimal maxScore)
    {
        if (value < 0 || value > maxScore)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Score must be between zero and the maximum score.");
        }

        Value = value;
    }

    public decimal Value { get; }
}
