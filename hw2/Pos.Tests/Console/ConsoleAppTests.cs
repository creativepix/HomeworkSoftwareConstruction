using Pos.Application;
using Pos.Console;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public class ConsoleAppTests
{
    [Fact]
    public void ExitCommandFinishesWithoutError()
    {
        var output = new StringWriter();
        var app = new ConsoleApp(new FakeFacade(), new StringReader("0\n"), output);

        app.Run();

        Assert.Contains("Выбор", output.ToString());
    }

    [Fact]
    public void EndOfInputFinishesWithoutError()
    {
        var app = new ConsoleApp(
            new FakeFacade(), new StringReader(string.Empty), new StringWriter());

        app.Run();
    }

    [Fact]
    public void ReplacedInputAndOutputCanRunStudentScenario()
    {
        var output = new StringWriter();
        var input = new StringReader("1\nИван Иванов\n2\n0\n");
        var app = new ConsoleApp(new FakeFacade(), input, output);

        app.Run();

        Assert.Contains("Студент добавлен", output.ToString());
        Assert.Contains("Иван Иванов", output.ToString());
    }

    [Fact]
    public void InvalidNumberAndUnknownCommandDoNotStopApplication()
    {
        var output = new StringWriter();
        var input = new StringReader("3\nТесты\nне число\nне команда\n0\n");
        var app = new ConsoleApp(new FakeFacade(), input, output);

        app.Run();

        Assert.Contains("Введите число", output.ToString());
        Assert.Contains("Неизвестная команда", output.ToString());
    }

    [Fact]
    public void ExpectedFacadeErrorIsPrintedAndMenuContinues()
    {
        var output = new StringWriter();
        var input = new StringReader("1\nИван Иванов\n1\nИван Иванов\n0\n");
        var app = new ConsoleApp(new FakeFacade(), input, output);

        app.Run();

        Assert.Contains("ОШИБКА", output.ToString());
        Assert.True(output.ToString().Split("Выбор").Length >= 3);
    }

    [Fact]
    public void FileErrorsArePrintedAndDoNotStopApplication()
    {
        var output = new StringWriter();
        var input = new StringReader("1\nИван Иванов\n9\nплохой путь\n0\n");
        var app = new ConsoleApp(new FakeFacade(true), input, output);

        app.Run();

        Assert.Contains("Ошибка файла", output.ToString());
        Assert.True(output.ToString().Split("Выбор").Length >= 3);
    }

    [Fact]
    public void LockedRealJsonFileDoesNotCrashConsoleScenario()
    {
        var directory = Path.Combine(
            AppContext.BaseDirectory, "TestFiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var jsonPath = Path.Combine(directory, "pos.json");

        try
        {
            var repository = new JsonPosRepository(jsonPath);
            var facade = new PosFacade(repository, repository, repository,
                new WeightedFinalScoreStrategy(), new CsvResultExporter());
            var output = new StringWriter();
            var app = new ConsoleApp(
                facade, new StringReader("1\nИван Иванов\n0\n"), output);

            using (new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                app.Run();
            }

            Assert.Contains("Ошибка файла", output.ToString());
            Assert.Empty(repository.GetAll());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private class FakeFacade : IPosFacade
    {
        private readonly List<StudentData> _students = new List<StudentData>();
        private readonly List<CriterionData> _criteria = new List<CriterionData>();
        private readonly bool _throwFileErrors;

        public FakeFacade(bool throwFileErrors = false)
        {
            _throwFileErrors = throwFileErrors;
        }

        public StudentData AddStudent(string name)
        {
            if (_throwFileErrors)
            {
                throw new IOException("Файл недоступен.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Имя не может быть пустым.");
            }

            if (_students.Any(student => student.Name == name))
            {
                throw new InvalidOperationException("Такой студент уже существует.");
            }

            var student = new StudentData(Guid.NewGuid(), name);
            _students.Add(student);
            return student;
        }

        public IReadOnlyList<StudentData> GetStudents()
        {
            return _students;
        }

        public CriterionData AddCriterion(string name, decimal maxScore, decimal weight)
        {
            var criterion = new CriterionData(Guid.NewGuid(), name, maxScore, weight);
            _criteria.Add(criterion);
            return criterion;
        }

        public IReadOnlyList<CriterionData> GetCriteria()
        {
            return _criteria;
        }

        public EvaluationData SetEvaluation(
            Guid studentId, Guid criterionId, decimal score, string? comment = null)
        {
            return new EvaluationData(
                Guid.NewGuid(), studentId, criterionId, score, DateTime.UtcNow, comment);
        }

        public EvaluationData? GetEvaluation(Guid studentId, Guid criterionId)
        {
            return null;
        }

        public bool DeleteEvaluation(Guid studentId, Guid criterionId)
        {
            return false;
        }

        public decimal GetFinalScore(Guid studentId)
        {
            return 0;
        }

        public void ExportResults(string path)
        {
            if (_throwFileErrors)
            {
                throw new IOException("Файл недоступен.");
            }
        }
    }
}
