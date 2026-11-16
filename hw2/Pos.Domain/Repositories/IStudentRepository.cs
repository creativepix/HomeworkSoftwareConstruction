namespace Pos.Domain;

public interface IStudentRepository
{
    Student? GetById(StudentId id);
    Student? GetByName(string name);
    IReadOnlyList<Student> GetAll();
    void Save(Student student);
}
