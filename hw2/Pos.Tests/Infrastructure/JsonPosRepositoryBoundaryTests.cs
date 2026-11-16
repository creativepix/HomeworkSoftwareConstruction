using Pos.Infrastructure;

namespace Pos.Tests;

public class JsonPosRepositoryBoundaryTests
{
    [Fact]
    public void MalformedJsonIsReportedAsInvalidData()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");
        File.WriteAllText(path, "{not valid json");

        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonPosRepository(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void NullJsonCollectionsAreTreatedAsEmpty()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");
        File.WriteAllText(path,
            """
            {
              "Students": null,
              "Criteria": null,
              "Evaluations": null
            }
            """);

        try
        {
            var repository = new JsonPosRepository(path);

            Assert.Empty(repository.GetAll());
            Assert.Empty(repository.GetCriteria());
            Assert.Empty(repository.GetAllEvaluations());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void JsonEvaluationWithUnknownStudentIsRejected()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");
        var criterionId = Guid.NewGuid();
        File.WriteAllText(path,
            $$"""
            {
              "Students": [],
              "Criteria": [
                {
                  "Id": "{{criterionId}}",
                  "Name": "Тесты",
                  "MaxScore": 10,
                  "Weight": 1
                }
              ],
              "Evaluations": [
                {
                  "Id": "{{Guid.NewGuid()}}",
                  "StudentId": "{{Guid.NewGuid()}}",
                  "CriterionId": "{{criterionId}}",
                  "Score": 5
                }
              ]
            }
            """);

        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonPosRepository(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void JsonWithDuplicateEvaluationsIsRejected()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");
        var studentId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();
        var firstEvaluationId = Guid.NewGuid();
        var secondEvaluationId = Guid.NewGuid();
        File.WriteAllText(path,
            $$"""
            {
              "Students": [
                { "Id": "{{studentId}}", "Name": "Иван Иванов" }
              ],
              "Criteria": [
                {
                  "Id": "{{criterionId}}",
                  "Name": "Тесты",
                  "MaxScore": 10,
                  "Weight": 1
                }
              ],
              "Evaluations": [
                {
                  "Id": "{{firstEvaluationId}}",
                  "StudentId": "{{studentId}}",
                  "CriterionId": "{{criterionId}}",
                  "Score": 5
                },
                {
                  "Id": "{{secondEvaluationId}}",
                  "StudentId": "{{studentId}}",
                  "CriterionId": "{{criterionId}}",
                  "Score": 7
                }
              ]
            }
            """);

        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonPosRepository(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void RepositoryRejectsEmptyPath()
    {
        Assert.Throws<ArgumentException>(() => new JsonPosRepository("   "));
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(
            AppContext.BaseDirectory, "TestFiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
