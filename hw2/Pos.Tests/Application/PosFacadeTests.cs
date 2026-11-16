using Pos.Application;
using Pos.Domain;

namespace Pos.Tests;

public class PosFacadeTests
{
    [Fact]
    public void AddStudentRejectsDuplicateName()
    {
        var storage = new MemoryStorage();
        var facade = CreateFacade(storage);
        facade.AddStudent("Иван Иванов");

        Assert.Throws<InvalidOperationException>(() => facade.AddStudent("иван иванов"));
    }

    [Fact]
    public void EvaluationCanBeViewedUpdatedAndDeleted()
    {
        var storage = new MemoryStorage();
        var facade = CreateFacade(storage);
        var student = facade.AddStudent("Иван Иванов");
        var criterion = facade.AddCriterion("Тесты", 10m, 1m);

        facade.SetEvaluation(student.Id, criterion.Id, 6m, "Первая попытка");
        facade.SetEvaluation(student.Id, criterion.Id, 9m, "Исправлено");
        var evaluation = facade.GetEvaluation(student.Id, criterion.Id);

        Assert.NotNull(evaluation);
        Assert.Equal(9m, evaluation.Score);
        Assert.Equal("Исправлено", evaluation.Comment);
        Assert.True(facade.DeleteEvaluation(student.Id, criterion.Id));
        Assert.Null(facade.GetEvaluation(student.Id, criterion.Id));
        Assert.False(facade.DeleteEvaluation(student.Id, criterion.Id));
    }

    [Fact]
    public void SetEvaluationRejectsUnknownCriterion()
    {
        var storage = new MemoryStorage();
        var facade = CreateFacade(storage);
        var student = facade.AddStudent("Иван Иванов");

        Assert.Throws<KeyNotFoundException>(() =>
            facade.SetEvaluation(student.Id, Guid.NewGuid(), 5m));
    }

    [Fact]
    public void FinalScoreUsesCurrentEvaluations()
    {
        var storage = new MemoryStorage();
        var facade = CreateFacade(storage);
        var student = facade.AddStudent("Иван Иванов");
        var first = facade.AddCriterion("Архитектура", 10m, 1m);
        var second = facade.AddCriterion("Тесты", 20m, 3m);
        facade.SetEvaluation(student.Id, first.Id, 10m);
        facade.SetEvaluation(student.Id, second.Id, 10m);

        Assert.Equal(6.25m, facade.GetFinalScore(student.Id));
    }

    [Fact]
    public void ExportPassesCalculatedResultsToExporter()
    {
        var storage = new MemoryStorage();
        var exporter = new CapturingExporter();
        var facade = new PosFacade(storage, storage, storage,
            new WeightedFinalScoreStrategy(), exporter);
        var student = facade.AddStudent("Иван Иванов");
        var criterion = facade.AddCriterion("Тесты", 10m, 1m);
        facade.SetEvaluation(student.Id, criterion.Id, 8m);

        facade.ExportResults("results.csv");

        Assert.Equal("results.csv", exporter.Path);
        var result = Assert.Single(exporter.Results);
        Assert.Equal(student.Id, result.StudentId);
        Assert.Equal("Иван Иванов", result.StudentName);
        Assert.Equal(8m, result.FinalScore);
    }

    private static PosFacade CreateFacade(MemoryStorage storage)
    {
        return new PosFacade(storage, storage, storage,
            new WeightedFinalScoreStrategy(), new CapturingExporter());
    }

    private class CapturingExporter : IResultExporter
    {
        public string? Path { get; private set; }
        public IReadOnlyList<StudentResultData> Results { get; private set; }
            = Array.Empty<StudentResultData>();

        public void Export(string path, IReadOnlyList<StudentResultData> results)
        {
            Path = path;
            Results = results;
        }
    }

    private class MemoryStorage : IStudentRepository, IRubricRepository, IEvaluationRepository
    {
        private readonly List<Student> _students = new List<Student>();
        private readonly List<Evaluation> _evaluations = new List<Evaluation>();
        private Rubric _rubric = new Rubric();

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
            _students.Add(student);
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
            _rubric = rubric;
        }

        public void SaveCriterion(Criterion criterion)
        {
            _rubric.AddCriterion(criterion);
        }

        public Evaluation? Get(StudentId studentId, CriterionId criterionId)
        {
            return _evaluations.FirstOrDefault(evaluation =>
                evaluation.StudentId == studentId && evaluation.CriterionId == criterionId);
        }

        public IReadOnlyList<Evaluation> GetByStudent(StudentId studentId)
        {
            return _evaluations.Where(evaluation => evaluation.StudentId == studentId).ToList();
        }

        public IReadOnlyList<Evaluation> GetAllEvaluations()
        {
            return _evaluations.AsReadOnly();
        }

        public void Save(Evaluation evaluation)
        {
            Delete(evaluation.StudentId, evaluation.CriterionId);
            _evaluations.Add(evaluation);
        }

        public bool Delete(StudentId studentId, CriterionId criterionId)
        {
            var evaluation = Get(studentId, criterionId);
            if (evaluation is null)
            {
                return false;
            }

            return _evaluations.Remove(evaluation);
        }
    }
}
