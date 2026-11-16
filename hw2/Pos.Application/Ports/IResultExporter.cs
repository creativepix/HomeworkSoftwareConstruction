namespace Pos.Application;

public interface IResultExporter
{
    void Export(string path, IReadOnlyList<StudentResultData> results);
}
