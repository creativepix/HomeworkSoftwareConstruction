namespace Pos.Domain;

public class Student
{
    public Student(StudentId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Student name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    public StudentId Id { get; }
    public string Name { get; }
}
