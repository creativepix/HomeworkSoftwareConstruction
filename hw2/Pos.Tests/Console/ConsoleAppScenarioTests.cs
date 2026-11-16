using Pos.Application;
using Pos.Console;

namespace Pos.Tests;

public class ConsoleAppScenarioTests
{
    [Fact]
    public void AllMenuCommandsWorkWithReplacedInputAndOutput()
    {
        var facade = new ScenarioFacade();
        var input = new StringReader(string.Join('\n', new[]
        {
            "2",
            "4",
            "1", "Иван Иванов",
            "2",
            "3", "Тесты", "10", "1",
            "4",
            "5", facade.StudentId.ToString(), facade.CriterionId.ToString(), "8", "Хорошо",
            "6", facade.StudentId.ToString(), facade.CriterionId.ToString(),
            "5", facade.StudentId.ToString(), facade.CriterionId.ToString(), "9", "",
            "6", facade.StudentId.ToString(), facade.CriterionId.ToString(),
            "7", facade.StudentId.ToString(), facade.CriterionId.ToString(),
            "6", facade.StudentId.ToString(), facade.CriterionId.ToString(),
            "7", facade.StudentId.ToString(), facade.CriterionId.ToString(),
            "8", facade.StudentId.ToString(),
            "9", "exports/results.csv",
            "0"
        }) + "\n");
        var output = new StringWriter();
        var app = new ConsoleApp(facade, input, output);

        app.Run();

        var text = output.ToString();
        Assert.Contains("Студентов пока нет", text);
        Assert.Contains("Критериев пока нет", text);
        Assert.Contains("Иван Иванов", text);
        Assert.Contains("Тесты", text);
        Assert.Contains("Оценка сохранена: 8", text);
        Assert.Contains("комментарий: Хорошо", text);
        Assert.Contains("комментарий: -", text);
        Assert.Contains("Оценка удалена", text);
        Assert.Contains("Оценка не найдена", text);
        Assert.Contains("Итог: 7.5 / 10", text);
        Assert.Contains("Итоги экспортированы", text);
        Assert.Equal("exports/results.csv", facade.ExportPath);
    }

    [Fact]
    public void InvalidIdsAndScoresReturnToMenuWithoutCallingFacade()
    {
        var facade = new ScenarioFacade();
        var input = new StringReader(string.Join('\n', new[]
        {
            "5", "не guid",
            "5", facade.StudentId.ToString(), "не guid",
            "5", facade.StudentId.ToString(), facade.CriterionId.ToString(), "не число",
            "6", "не guid",
            "7", "не guid",
            "8", "не guid",
            "0"
        }) + "\n");
        var output = new StringWriter();
        var app = new ConsoleApp(facade, input, output);

        app.Run();

        Assert.Contains("Введите корректный Id", output.ToString());
        Assert.Contains("Введите число", output.ToString());
        Assert.Equal(0, facade.SetEvaluationCalls);
    }

    [Theory]
    [InlineData("1\n")]
    [InlineData("3\n")]
    [InlineData("9\n")]
    public void EndOfInputInsideCommandDoesNotCrash(string input)
    {
        var app = new ConsoleApp(
            new ScenarioFacade(), new StringReader(input), new StringWriter());

        app.Run();
    }

    [Fact]
    public void KeyNotFoundErrorIsPrintedAndMenuContinues()
    {
        var facade = new ScenarioFacade { ThrowKeyNotFound = true };
        var output = new StringWriter();
        var app = new ConsoleApp(
            facade,
            new StringReader($"8\n{facade.StudentId}\n0\n"),
            output);

        app.Run();

        Assert.Contains("Студент не найден", output.ToString());
    }

    [Fact]
    public void UnauthorizedAccessErrorIsPrintedAndMenuContinues()
    {
        var facade = new ScenarioFacade { ThrowUnauthorizedAccess = true };
        var output = new StringWriter();
        var app = new ConsoleApp(
            facade,
            new StringReader("9\nresults.csv\n0\n"),
            output);

        app.Run();

        Assert.Contains("Нет доступа к файлу", output.ToString());
    }

    private sealed class ScenarioFacade : IPosFacade
    {
        private bool _studentAdded;
        private bool _criterionAdded;
        private EvaluationData? _evaluation;

        public Guid StudentId { get; } = Guid.NewGuid();
        public Guid CriterionId { get; } = Guid.NewGuid();
        public string? ExportPath { get; private set; }
        public int SetEvaluationCalls { get; private set; }
        public bool ThrowKeyNotFound { get; init; }
        public bool ThrowUnauthorizedAccess { get; init; }

        public StudentData AddStudent(string name)
        {
            _studentAdded = true;
            return new StudentData(StudentId, name);
        }

        public IReadOnlyList<StudentData> GetStudents()
        {
            if (!_studentAdded)
            {
                return Array.Empty<StudentData>();
            }

            return new[] { new StudentData(StudentId, "Иван Иванов") };
        }

        public CriterionData AddCriterion(string name, decimal maxScore, decimal weight)
        {
            _criterionAdded = true;
            return new CriterionData(CriterionId, name, maxScore, weight);
        }

        public IReadOnlyList<CriterionData> GetCriteria()
        {
            if (!_criterionAdded)
            {
                return Array.Empty<CriterionData>();
            }

            return new[] { new CriterionData(CriterionId, "Тесты", 10m, 1m) };
        }

        public EvaluationData SetEvaluation(
            Guid studentId,
            Guid criterionId,
            decimal score,
            string? comment = null)
        {
            SetEvaluationCalls++;
            _evaluation = new EvaluationData(
                Guid.NewGuid(), studentId, criterionId, score, DateTime.UtcNow, comment);
            return _evaluation;
        }

        public EvaluationData? GetEvaluation(Guid studentId, Guid criterionId)
        {
            return _evaluation;
        }

        public bool DeleteEvaluation(Guid studentId, Guid criterionId)
        {
            if (_evaluation is null)
            {
                return false;
            }

            _evaluation = null;
            return true;
        }

        public decimal GetFinalScore(Guid studentId)
        {
            if (ThrowKeyNotFound)
            {
                throw new KeyNotFoundException("Студент не найден.");
            }

            return 7.5m;
        }

        public void ExportResults(string path)
        {
            if (ThrowUnauthorizedAccess)
            {
                throw new UnauthorizedAccessException("Нет прав.");
            }

            ExportPath = path;
        }
    }
}
