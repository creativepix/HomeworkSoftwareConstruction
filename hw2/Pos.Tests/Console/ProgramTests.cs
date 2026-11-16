using Pos.Console;

namespace Pos.Tests;

[CollectionDefinition("GlobalConsole", DisableParallelization = true)]
public class GlobalConsoleCollection
{
}

[Collection("GlobalConsole")]
public class ProgramTests
{
    [Fact]
    public void MainBuildsDependenciesAndExitsNormally()
    {
        var state = SaveConsoleState();
        var output = new StringWriter();

        try
        {
            System.Console.SetIn(new StringReader("0\n"));
            System.Console.SetOut(output);
            Program.Main();
            Assert.Contains("Выбор", output.ToString());
        }
        finally
        {
            state.Restore();
        }
    }

    [Fact]
    public void MainReportsMalformedJsonWithoutUnhandledException()
    {
        RunWithInvalidData("{not valid json");
    }

    [Theory]
    [MemberData(nameof(InvalidDomainJsonCases))]
    public void MainReportsInvalidDomainJsonWithoutUnhandledException(string json)
    {
        RunWithInvalidData(json);
    }

    [Fact]
    public void MainReportsLockedJsonWithoutUnhandledException()
    {
        var state = SaveConsoleState();
        var error = new StringWriter();
        var dataPath = GetDataPath();
        Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);
        var oldData = File.Exists(dataPath) ? File.ReadAllText(dataPath) : null;
        File.WriteAllText(dataPath, "{\"Students\":[],\"Criteria\":[],\"Evaluations\":[]}");

        try
        {
            System.Console.SetIn(new StringReader(string.Empty));
            System.Console.SetError(error);
            using (new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Program.Main();
            }

            Assert.Contains("ОШИБКА ЗАПУСКА", error.ToString());
        }
        finally
        {
            RestoreData(dataPath, oldData);
            state.Restore();
        }
    }

    public static IEnumerable<object[]> InvalidDomainJsonCases()
    {
        var firstCriterionId = Guid.NewGuid();
        var secondCriterionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        yield return new object[]
        {
            $$"""
            {
              "Students": [],
              "Criteria": [
                { "Id": "{{firstCriterionId}}", "Name": "Тесты", "MaxScore": 0, "Weight": 1 }
              ],
              "Evaluations": []
            }
            """
        };
        yield return new object[]
        {
            $$"""
            {
              "Students": [],
              "Criteria": [
                { "Id": "{{firstCriterionId}}", "Name": "Тесты", "MaxScore": 10, "Weight": 1 },
                { "Id": "{{secondCriterionId}}", "Name": "тесты", "MaxScore": 10, "Weight": 1 }
              ],
              "Evaluations": []
            }
            """
        };
        yield return new object[]
        {
            $$"""
            {
              "Students": [
                { "Id": "{{studentId}}", "Name": "Иван Иванов" }
              ],
              "Criteria": [],
              "Evaluations": [
                {
                  "Id": "{{Guid.NewGuid()}}",
                  "StudentId": "{{studentId}}",
                  "CriterionId": "{{firstCriterionId}}",
                  "Score": 5
                }
              ]
            }
            """
        };
    }

    private static void RunWithInvalidData(string json)
    {
        var state = SaveConsoleState();
        var error = new StringWriter();
        var dataPath = GetDataPath();
        Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);
        var oldData = File.Exists(dataPath) ? File.ReadAllText(dataPath) : null;
        File.WriteAllText(dataPath, json);

        try
        {
            System.Console.SetIn(new StringReader(string.Empty));
            System.Console.SetError(error);
            Program.Main();
            Assert.Contains("ОШИБКА ЗАПУСКА", error.ToString());
        }
        finally
        {
            RestoreData(dataPath, oldData);
            state.Restore();
        }
    }

    private static string GetDataPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "data", "pos-data.json");
    }

    private static void RestoreData(string dataPath, string? oldData)
    {
        if (oldData is null)
        {
            File.Delete(dataPath);
        }
        else
        {
            File.WriteAllText(dataPath, oldData);
        }
    }

    private static ConsoleState SaveConsoleState()
    {
        return new ConsoleState(System.Console.In, System.Console.Out, System.Console.Error);
    }

    private sealed class ConsoleState
    {
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly TextWriter _error;

        public ConsoleState(TextReader input, TextWriter output, TextWriter error)
        {
            _input = input;
            _output = output;
            _error = error;
        }

        public void Restore()
        {
            System.Console.SetIn(_input);
            System.Console.SetOut(_output);
            System.Console.SetError(_error);
        }
    }
}
