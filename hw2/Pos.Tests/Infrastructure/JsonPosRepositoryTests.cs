using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public class JsonPosRepositoryTests
{
    [Fact]
    public void RepositoryWritesAndReloadsRealJsonFile()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos data.json");

        try
        {
            var repository = new JsonPosRepository(path);
            var student = new Student(new StudentId(Guid.NewGuid()), "Иван Иванов");
            repository.Save(student);
            var afterStudent = File.ReadAllText(path);

            var criterion = new Criterion(
                new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m);
            repository.SaveCriterion(criterion);
            var afterCriterion = File.ReadAllText(path);

            var evaluation = new EvaluationFactory(repository.Get()!).Create(
                new EvaluationId(Guid.NewGuid()), student.Id, criterion.Id, 8m);
            repository.Save(evaluation);
            var afterEvaluation = File.ReadAllText(path);

            Assert.NotEqual(afterStudent, afterCriterion);
            Assert.NotEqual(afterCriterion, afterEvaluation);

            var loaded = new JsonPosRepository(path);
            Assert.Equal("Иван Иванов", loaded.GetById(student.Id)?.Name);
            Assert.Equal("Тесты", loaded.GetCriterion(criterion.Id)?.Name);
            Assert.Equal(8m, loaded.Get(student.Id, criterion.Id)?.Score.Value);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void EvaluationUpdateAndDeleteChangeJsonFile()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");

        try
        {
            var repository = new JsonPosRepository(path);
            var student = new Student(new StudentId(Guid.NewGuid()), "Иван Иванов");
            var criterion = new Criterion(
                new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m);
            repository.Save(student);
            repository.SaveCriterion(criterion);
            var factory = new EvaluationFactory(repository.Get()!);
            repository.Save(factory.Create(
                new EvaluationId(Guid.NewGuid()), student.Id, criterion.Id, 5m));
            var beforeUpdate = File.ReadAllText(path);

            repository.Save(factory.Create(
                new EvaluationId(Guid.NewGuid()), student.Id, criterion.Id, 9m));
            var afterUpdate = File.ReadAllText(path);
            var deleted = repository.Delete(student.Id, criterion.Id);
            var afterDelete = File.ReadAllText(path);

            Assert.NotEqual(beforeUpdate, afterUpdate);
            Assert.NotEqual(afterUpdate, afterDelete);
            Assert.True(deleted);
            Assert.Null(repository.Get(student.Id, criterion.Id));
            Assert.False(repository.Delete(student.Id, criterion.Id));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void RepositoryRejectsDuplicateStudentAndCriterionNames()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");

        try
        {
            var repository = new JsonPosRepository(path);
            repository.Save(new Student(new StudentId(Guid.NewGuid()), "Иван Иванов"));
            repository.SaveCriterion(new Criterion(
                new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m));

            Assert.Throws<InvalidOperationException>(() => repository.Save(
                new Student(new StudentId(Guid.NewGuid()), "иван иванов")));
            Assert.Throws<InvalidOperationException>(() => repository.SaveCriterion(
                new Criterion(new CriterionId(Guid.NewGuid()), "тесты", 10m, 1m)));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void LoadingJsonRestoresDomainInvariants()
    {
        var directory = CreateTestDirectory();
        var path = Path.Combine(directory, "pos.json");

        try
        {
            var repository = new JsonPosRepository(path);
            var student = new Student(new StudentId(Guid.NewGuid()), "Иван Иванов");
            var criterion = new Criterion(
                new CriterionId(Guid.NewGuid()), "Тесты", 10m, 1m);
            repository.Save(student);
            repository.SaveCriterion(criterion);
            repository.Save(new EvaluationFactory(repository.Get()!).Create(
                new EvaluationId(Guid.NewGuid()), student.Id, criterion.Id, 5m));
            var json = File.ReadAllText(path).Replace("\"Score\": 5", "\"Score\": 50");
            File.WriteAllText(path, json);

            Assert.Throws<ArgumentOutOfRangeException>(() => new JsonPosRepository(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(
            AppContext.BaseDirectory, "TestFiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
