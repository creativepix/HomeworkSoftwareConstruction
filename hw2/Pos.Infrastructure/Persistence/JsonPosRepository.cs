using System.Text.Json;
using Pos.Domain;

namespace Pos.Infrastructure;

public class JsonPosRepository : IStudentRepository, IRubricRepository, IEvaluationRepository
{
    private readonly string _filePath;
    private List<Student> _students = new List<Student>();
    private List<Evaluation> _evaluations = new List<Evaluation>();
    private Rubric _rubric = new Rubric();

    public JsonPosRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("JSON file path cannot be empty.", nameof(filePath));
        }

        _filePath = filePath;
        if (File.Exists(_filePath))
        {
            Load();
        }
        else
        {
            WriteState(_students, _rubric, _evaluations);
        }
    }

    public Student? GetById(StudentId id)
    {
        return _students.FirstOrDefault(student => student.Id == id);
    }

    public Student? GetByName(string name)
    {
        return _students.FirstOrDefault(student =>
            string.Equals(student.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<Student> GetAll()
    {
        return _students.AsReadOnly();
    }

    public void Save(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        if (GetById(student.Id) is not null)
        {
            throw new InvalidOperationException("A student with this identifier already exists.");
        }

        if (GetByName(student.Name) is not null)
        {
            throw new InvalidOperationException("A student with this name already exists.");
        }

        var students = new List<Student>(_students) { student };
        WriteState(students, _rubric, _evaluations);
        _students = students;
    }

    public Rubric? Get()
    {
        return _rubric;
    }

    public Criterion? GetCriterion(CriterionId id)
    {
        return _rubric.Criteria.FirstOrDefault(criterion => criterion.Id == id);
    }

    public IReadOnlyList<Criterion> GetCriteria()
    {
        return _rubric.Criteria;
    }

    public void Save(Rubric rubric)
    {
        ArgumentNullException.ThrowIfNull(rubric);
        WriteState(_students, rubric, _evaluations);
        _rubric = rubric;
    }

    public void SaveCriterion(Criterion criterion)
    {
        var rubric = CopyRubric(_rubric);
        rubric.AddCriterion(criterion);
        WriteState(_students, rubric, _evaluations);
        _rubric = rubric;
    }

    public Evaluation? Get(StudentId studentId, CriterionId criterionId)
    {
        return _evaluations.FirstOrDefault(evaluation =>
            evaluation.StudentId == studentId && evaluation.CriterionId == criterionId);
    }

    public IReadOnlyList<Evaluation> GetByStudent(StudentId studentId)
    {
        return _evaluations
            .Where(evaluation => evaluation.StudentId == studentId)
            .ToList();
    }

    public IReadOnlyList<Evaluation> GetAllEvaluations()
    {
        return _evaluations.AsReadOnly();
    }

    public void Save(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        if (GetById(evaluation.StudentId) is null)
        {
            throw new KeyNotFoundException("Student was not found.");
        }

        _rubric.GetCriterion(evaluation.CriterionId);

        var evaluations = _evaluations
            .Where(current => current.StudentId != evaluation.StudentId ||
                              current.CriterionId != evaluation.CriterionId)
            .ToList();
        evaluations.Add(evaluation);

        WriteState(_students, _rubric, evaluations);
        _evaluations = evaluations;
    }

    public bool Delete(StudentId studentId, CriterionId criterionId)
    {
        var evaluation = Get(studentId, criterionId);
        if (evaluation is null)
        {
            return false;
        }

        var evaluations = new List<Evaluation>(_evaluations);
        evaluations.Remove(evaluation);
        WriteState(_students, _rubric, evaluations);
        _evaluations = evaluations;
        return true;
    }

    private void Load()
    {
        var json = File.ReadAllText(_filePath);
        StorageModel model;
        try
        {
            model = JsonSerializer.Deserialize<StorageModel>(json) ?? new StorageModel();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("JSON file has an invalid format.", exception);
        }

        model.Students ??= new List<StudentRecord>();
        model.Criteria ??= new List<CriterionRecord>();
        model.Evaluations ??= new List<EvaluationRecord>();

        var students = new List<Student>();
        foreach (var item in model.Students)
        {
            var student = new Student(new StudentId(item.Id), item.Name);
            if (students.Any(current => current.Id == student.Id ||
                string.Equals(current.Name, student.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDataException("JSON contains duplicate students.");
            }

            students.Add(student);
        }

        var rubric = new Rubric();
        foreach (var item in model.Criteria)
        {
            rubric.AddCriterion(new Criterion(
                new CriterionId(item.Id), item.Name, item.MaxScore, item.Weight));
        }

        var evaluations = new List<Evaluation>();
        var factory = new EvaluationFactory(rubric);
        foreach (var item in model.Evaluations)
        {
            var studentId = new StudentId(item.StudentId);
            if (students.All(student => student.Id != studentId))
            {
                throw new InvalidDataException("JSON evaluation refers to an unknown student.");
            }

            if (evaluations.Any(current =>
                current.StudentId == studentId &&
                current.CriterionId == new CriterionId(item.CriterionId)))
            {
                throw new InvalidDataException("JSON contains duplicate evaluations.");
            }

            evaluations.Add(factory.Create(
                new EvaluationId(item.Id),
                studentId,
                new CriterionId(item.CriterionId),
                item.Score,
                item.Date,
                item.Comment));
        }

        _students = students;
        _rubric = rubric;
        _evaluations = evaluations;
    }

    private void WriteState(
        IReadOnlyList<Student> students,
        Rubric rubric,
        IReadOnlyList<Evaluation> evaluations)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var model = new StorageModel();
        foreach (var student in students)
        {
            model.Students.Add(new StudentRecord
            {
                Id = student.Id.Value,
                Name = student.Name
            });
        }

        foreach (var criterion in rubric.Criteria)
        {
            model.Criteria.Add(new CriterionRecord
            {
                Id = criterion.Id.Value,
                Name = criterion.Name,
                MaxScore = criterion.MaxScore,
                Weight = criterion.Weight
            });
        }

        foreach (var evaluation in evaluations)
        {
            model.Evaluations.Add(new EvaluationRecord
            {
                Id = evaluation.Id.Value,
                StudentId = evaluation.StudentId.Value,
                CriterionId = evaluation.CriterionId.Value,
                Score = evaluation.Score.Value,
                Date = evaluation.Date,
                Comment = evaluation.Comment
            });
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_filePath, JsonSerializer.Serialize(model, options));
    }

    private static Rubric CopyRubric(Rubric source)
    {
        var result = new Rubric();
        foreach (var criterion in source.Criteria)
        {
            result.AddCriterion(criterion);
        }

        return result;
    }

    private class StorageModel
    {
        public List<StudentRecord> Students { get; set; } = new List<StudentRecord>();
        public List<CriterionRecord> Criteria { get; set; } = new List<CriterionRecord>();
        public List<EvaluationRecord> Evaluations { get; set; } = new List<EvaluationRecord>();
    }

    private class StudentRecord
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class CriterionRecord
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public decimal Weight { get; set; }
    }

    private class EvaluationRecord
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid CriterionId { get; set; }
        public decimal Score { get; set; }
        public DateTime? Date { get; set; }
        public string? Comment { get; set; }
    }
}
