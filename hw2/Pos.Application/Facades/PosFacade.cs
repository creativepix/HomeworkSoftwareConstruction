using Pos.Domain;

namespace Pos.Application;

public class PosFacade : IPosFacade
{
    private readonly IStudentRepository _studentRepository;
    private readonly IRubricRepository _rubricRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IFinalScoreStrategy _finalScoreStrategy;
    private readonly IResultExporter _resultExporter;

    public PosFacade(
        IStudentRepository studentRepository,
        IRubricRepository rubricRepository,
        IEvaluationRepository evaluationRepository,
        IFinalScoreStrategy finalScoreStrategy,
        IResultExporter resultExporter)
    {
        _studentRepository = studentRepository;
        _rubricRepository = rubricRepository;
        _evaluationRepository = evaluationRepository;
        _finalScoreStrategy = finalScoreStrategy;
        _resultExporter = resultExporter;
    }

    public StudentData AddStudent(string name)
    {
        if (_studentRepository.GetByName(name) is not null)
        {
            throw new InvalidOperationException("A student with this name already exists.");
        }

        var student = new Student(new StudentId(Guid.NewGuid()), name);
        _studentRepository.Save(student);
        return ToData(student);
    }

    public IReadOnlyList<StudentData> GetStudents()
    {
        var result = new List<StudentData>();
        foreach (var student in _studentRepository.GetAll())
        {
            result.Add(ToData(student));
        }

        return result;
    }

    public CriterionData AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(new CriterionId(Guid.NewGuid()), name, maxScore, weight);
        _rubricRepository.SaveCriterion(criterion);
        return ToData(criterion);
    }

    public IReadOnlyList<CriterionData> GetCriteria()
    {
        var result = new List<CriterionData>();
        foreach (var criterion in _rubricRepository.GetCriteria())
        {
            result.Add(ToData(criterion));
        }

        return result;
    }

    public EvaluationData SetEvaluation(
        Guid studentId,
        Guid criterionId,
        decimal score,
        string? comment = null)
    {
        var checkedStudentId = new StudentId(studentId);
        if (_studentRepository.GetById(checkedStudentId) is null)
        {
            throw new KeyNotFoundException("Student was not found.");
        }

        var checkedCriterionId = new CriterionId(criterionId);
        var rubric = _rubricRepository.Get() ?? new Rubric();
        var current = _evaluationRepository.Get(checkedStudentId, checkedCriterionId);
        var evaluationId = current?.Id ?? new EvaluationId(Guid.NewGuid());
        var factory = new EvaluationFactory(rubric);
        var evaluation = factory.Create(
            evaluationId,
            checkedStudentId,
            checkedCriterionId,
            score,
            DateTime.UtcNow,
            comment);

        _evaluationRepository.Save(evaluation);
        return ToData(evaluation);
    }

    public EvaluationData? GetEvaluation(Guid studentId, Guid criterionId)
    {
        var evaluation = _evaluationRepository.Get(
            new StudentId(studentId), new CriterionId(criterionId));
        return evaluation is null ? null : ToData(evaluation);
    }

    public bool DeleteEvaluation(Guid studentId, Guid criterionId)
    {
        return _evaluationRepository.Delete(
            new StudentId(studentId), new CriterionId(criterionId));
    }

    public decimal GetFinalScore(Guid studentId)
    {
        var checkedStudentId = new StudentId(studentId);
        if (_studentRepository.GetById(checkedStudentId) is null)
        {
            throw new KeyNotFoundException("Student was not found.");
        }

        var rubric = _rubricRepository.Get() ?? new Rubric();
        var evaluations = _evaluationRepository.GetByStudent(checkedStudentId);
        return _finalScoreStrategy.Calculate(checkedStudentId, rubric, evaluations);
    }

    public void ExportResults(string path)
    {
        var results = new List<StudentResultData>();
        foreach (var student in _studentRepository.GetAll())
        {
            var finalScore = GetFinalScore(student.Id.Value);
            results.Add(new StudentResultData(student.Id.Value, student.Name, finalScore));
        }

        _resultExporter.Export(path, results);
    }

    private static StudentData ToData(Student student)
    {
        return new StudentData(student.Id.Value, student.Name);
    }

    private static CriterionData ToData(Criterion criterion)
    {
        return new CriterionData(
            criterion.Id.Value,
            criterion.Name,
            criterion.MaxScore,
            criterion.Weight);
    }

    private static EvaluationData ToData(Evaluation evaluation)
    {
        return new EvaluationData(
            evaluation.Id.Value,
            evaluation.StudentId.Value,
            evaluation.CriterionId.Value,
            evaluation.Score.Value,
            evaluation.Date,
            evaluation.Comment);
    }
}
