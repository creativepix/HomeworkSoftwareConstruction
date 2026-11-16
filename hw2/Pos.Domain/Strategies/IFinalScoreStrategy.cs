namespace Pos.Domain;

public interface IFinalScoreStrategy
{
    decimal Calculate(StudentId studentId, Rubric rubric, IEnumerable<Evaluation> evaluations);
}
