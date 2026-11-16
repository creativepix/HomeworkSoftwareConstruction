namespace Pos.Application;

public sealed record StudentData(Guid Id, string Name);

public sealed record CriterionData(Guid Id, string Name, decimal MaxScore, decimal Weight);

public sealed record EvaluationData(
    Guid Id,
    Guid StudentId,
    Guid CriterionId,
    decimal Score,
    DateTime? Date,
    string? Comment);

public sealed record StudentResultData(Guid StudentId, string StudentName, decimal FinalScore);
