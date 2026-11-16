using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public class CsvResultExporter : IResultExporter
{
    public void Export(string path, IReadOnlyList<StudentResultData> results)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("CSV file path cannot be empty.", nameof(path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = new List<string> { "StudentId,StudentName,FinalScore" };
        foreach (var result in results)
        {
            lines.Add(string.Join(",",
                result.StudentId.ToString(),
                Escape(result.StudentName),
                result.FinalScore.ToString(CultureInfo.InvariantCulture)));
        }

        File.WriteAllLines(path, lines, new UTF8Encoding(false));
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') &&
            !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
