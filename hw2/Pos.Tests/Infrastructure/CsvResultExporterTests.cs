using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public class CsvResultExporterTests
{
    [Fact]
    public void ExporterWritesCalculatedResultsToRealCsvFile()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "папка с пробелом", "итоги, группы.csv");

        try
        {
            var exporter = new CsvResultExporter();
            var results = new[]
            {
                new StudentResultData(Guid.NewGuid(), "Иван, Иванов", 8.25m),
                new StudentResultData(Guid.NewGuid(), "Анна \"Аня\"", 10m)
            };

            exporter.Export(path, results);

            var csv = File.ReadAllText(path);
            Assert.Contains("StudentId,StudentName,FinalScore", csv);
            Assert.Contains("\"Иван, Иванов\",8.25", csv);
            Assert.Contains("\"Анна \"\"Аня\"\"\"", csv);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void FacadeScenarioExportsFinalScore()
    {
        var directory = CreateTestDirectory();
        var jsonPath = Path.Combine(directory, "pos.json");
        var csvPath = Path.Combine(directory, "results.csv");

        try
        {
            var repository = new JsonPosRepository(jsonPath);
            var facade = new PosFacade(repository, repository, repository,
                new WeightedFinalScoreStrategy(), new CsvResultExporter());
            var student = facade.AddStudent("Иван Иванов");
            var first = facade.AddCriterion("Архитектура", 10m, 1m);
            var second = facade.AddCriterion("Тесты", 20m, 3m);
            facade.SetEvaluation(student.Id, first.Id, 10m);
            facade.SetEvaluation(student.Id, second.Id, 10m);

            facade.ExportResults(csvPath);

            var csv = File.ReadAllText(csvPath);
            Assert.Contains("Иван Иванов,6.25", csv);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ExporterRejectsEmptyPath()
    {
        var exporter = new CsvResultExporter();

        Assert.Throws<ArgumentException>(() =>
            exporter.Export("   ", Array.Empty<StudentResultData>()));
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(
            AppContext.BaseDirectory, "TestFiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
