using System.Globalization;
using Pos.Application;

namespace Pos.Console;

public class ConsoleApp
{
    private readonly IPosFacade _facade;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleApp(IPosFacade facade, TextReader input, TextWriter output)
    {
        _facade = facade;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            var command = _input.ReadLine();
            if (command is null || command.Trim() == "0")
            {
                return;
            }

            try
            {
                RunCommand(command.Trim());
            }
            catch (ArgumentException exception)
            {
                PrintError(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                PrintError(exception.Message);
            }
            catch (KeyNotFoundException exception)
            {
                PrintError(exception.Message);
            }
            catch (IOException exception)
            {
                PrintError($"Ошибка файла: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                PrintError($"Нет доступа к файлу: {exception.Message}");
            }
        }
    }

    private void RunCommand(string command)
    {
        if (command == "1")
        {
            AddStudent();
        }
        else if (command == "2")
        {
            ShowStudents();
        }
        else if (command == "3")
        {
            AddCriterion();
        }
        else if (command == "4")
        {
            ShowCriteria();
        }
        else if (command == "5")
        {
            SetEvaluation();
        }
        else if (command == "6")
        {
            ShowEvaluation();
        }
        else if (command == "7")
        {
            DeleteEvaluation();
        }
        else if (command == "8")
        {
            ShowFinalScore();
        }
        else if (command == "9")
        {
            ExportResults();
        }
        else
        {
            PrintError("Неизвестная команда.");
        }
    }

    private void AddStudent()
    {
        var name = ReadText("Имя студента: ");
        if (name is null)
        {
            return;
        }

        var student = _facade.AddStudent(name);
        _output.WriteLine($"Студент добавлен: {student.Id}");
    }

    private void ShowStudents()
    {
        var students = _facade.GetStudents();
        if (students.Count == 0)
        {
            _output.WriteLine("Студентов пока нет.");
            return;
        }

        foreach (var student in students)
        {
            _output.WriteLine($"{student.Id}: {student.Name}");
        }
    }

    private void AddCriterion()
    {
        var name = ReadText("Название критерия: ");
        if (name is null || !TryReadDecimal("Максимальный балл: ", out var maxScore) ||
            !TryReadDecimal("Вес: ", out var weight))
        {
            return;
        }

        var criterion = _facade.AddCriterion(name, maxScore, weight);
        _output.WriteLine($"Критерий добавлен: {criterion.Id}");
    }

    private void ShowCriteria()
    {
        var criteria = _facade.GetCriteria();
        if (criteria.Count == 0)
        {
            _output.WriteLine("Критериев пока нет.");
            return;
        }

        foreach (var criterion in criteria)
        {
            _output.WriteLine(
                $"{criterion.Id}: {criterion.Name}, максимум {criterion.MaxScore}, вес {criterion.Weight}");
        }
    }

    private void SetEvaluation()
    {
        if (!TryReadGuid("Id студента: ", out var studentId) ||
            !TryReadGuid("Id критерия: ", out var criterionId) ||
            !TryReadDecimal("Балл: ", out var score))
        {
            return;
        }

        var comment = ReadText("Комментарий (можно оставить пустым): ");
        var evaluation = _facade.SetEvaluation(
            studentId, criterionId, score, string.IsNullOrWhiteSpace(comment) ? null : comment);
        _output.WriteLine($"Оценка сохранена: {evaluation.Score}");
    }

    private void ShowEvaluation()
    {
        if (!TryReadGuid("Id студента: ", out var studentId) ||
            !TryReadGuid("Id критерия: ", out var criterionId))
        {
            return;
        }

        var evaluation = _facade.GetEvaluation(studentId, criterionId);
        if (evaluation is null)
        {
            _output.WriteLine("Оценка не найдена.");
            return;
        }

        _output.WriteLine($"Балл: {evaluation.Score}; комментарий: {evaluation.Comment ?? "-"}");
    }

    private void DeleteEvaluation()
    {
        if (!TryReadGuid("Id студента: ", out var studentId) ||
            !TryReadGuid("Id критерия: ", out var criterionId))
        {
            return;
        }

        var deleted = _facade.DeleteEvaluation(studentId, criterionId);
        _output.WriteLine(deleted ? "Оценка удалена." : "Оценка не найдена.");
    }

    private void ShowFinalScore()
    {
        if (!TryReadGuid("Id студента: ", out var studentId))
        {
            return;
        }

        var result = _facade.GetFinalScore(studentId);
        _output.WriteLine($"Итог: {result.ToString(CultureInfo.InvariantCulture)} / 10");
    }

    private void ExportResults()
    {
        var path = ReadText("Путь к CSV: ");
        if (path is null)
        {
            return;
        }

        _facade.ExportResults(path);
        _output.WriteLine("Итоги экспортированы.");
    }

    private string? ReadText(string prompt)
    {
        _output.Write(prompt);
        return _input.ReadLine();
    }

    private bool TryReadGuid(string prompt, out Guid value)
    {
        var text = ReadText(prompt);
        if (!Guid.TryParse(text, out value))
        {
            PrintError("Введите корректный Id.");
            return false;
        }

        return true;
    }

    private bool TryReadDecimal(string prompt, out decimal value)
    {
        var text = ReadText(prompt);
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) &&
            !decimal.TryParse(text, out value))
        {
            PrintError("Введите число.");
            return false;
        }

        return true;
    }

    private void PrintMenu()
    {
        _output.WriteLine();
        _output.WriteLine("1 - добавить студента");
        _output.WriteLine("2 - список студентов");
        _output.WriteLine("3 - добавить критерий");
        _output.WriteLine("4 - список критериев");
        _output.WriteLine("5 - выставить или исправить оценку");
        _output.WriteLine("6 - посмотреть оценку");
        _output.WriteLine("7 - отменить оценку");
        _output.WriteLine("8 - получить итог");
        _output.WriteLine("9 - экспортировать итоги в CSV");
        _output.WriteLine("0 - выход");
        _output.Write("Выбор: ");
    }

    private void PrintError(string message)
    {
        _output.WriteLine($"ОШИБКА: {message}");
    }
}
