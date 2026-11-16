using Pos.Domain;

namespace Pos.Tests;

public class StudentTests
{
    [Fact]
    public void StudentKeepsIdAndName()
    {
        var id = new StudentId(Guid.NewGuid());

        var student = new Student(id, "Иван Иванов");

        Assert.Equal(id, student.Id);
        Assert.Equal("Иван Иванов", student.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void StudentRejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() => new Student(new StudentId(Guid.NewGuid()), name));
    }

    [Fact]
    public void StudentRejectsNullName()
    {
        Assert.Throws<ArgumentException>(() => new Student(new StudentId(Guid.NewGuid()), null!));
    }
}
