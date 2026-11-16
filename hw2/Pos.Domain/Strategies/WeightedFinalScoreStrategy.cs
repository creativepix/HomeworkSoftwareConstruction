namespace Pos.Domain;

public class WeightedFinalScoreStrategy : IFinalScoreStrategy
{
    public decimal Calculate(
        StudentId studentId,
        Rubric rubric,
        IEnumerable<Evaluation> evaluations)
    {
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(evaluations);

        var evaluationList = evaluations.ToList();
        decimal weightedScore = 0;
        decimal totalWeight = 0;

        foreach (var criterion in rubric.Criteria)
        {
            totalWeight += criterion.Weight;

            foreach (var evaluation in evaluationList)
            {
                if (evaluation.StudentId == studentId && evaluation.CriterionId == criterion.Id)
                {
                    weightedScore += evaluation.Score.Value / criterion.MaxScore * criterion.Weight;
                    break;
                }
            }
        }

        if (totalWeight == 0)
        {
            return 0;
        }

        return weightedScore / totalWeight * 10;
    }
}
