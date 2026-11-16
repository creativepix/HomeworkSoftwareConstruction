using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Console;

public static class Program
{
    public static void Main()
    {
        try
        {
            var services = new ServiceCollection();
            var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "pos-data.json");
            var jsonPosRepository = new JsonPosRepository(dataPath);

            services.AddSingleton(jsonPosRepository);
            services.AddSingleton<IStudentRepository>(jsonPosRepository);
            services.AddSingleton<IRubricRepository>(jsonPosRepository);
            services.AddSingleton<IEvaluationRepository>(jsonPosRepository);
            services.AddSingleton<IFinalScoreStrategy, WeightedFinalScoreStrategy>();
            services.AddSingleton<IResultExporter, CsvResultExporter>();
            services.AddSingleton<IPosFacade, PosFacade>();
            services.AddSingleton<TextReader>(System.Console.In);
            services.AddSingleton<TextWriter>(System.Console.Out);
            services.AddSingleton<ConsoleApp>();

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<ConsoleApp>().Run();
        }
        catch (InvalidDataException exception)
        {
            PrintStartupError(exception);
        }
        catch (IOException exception)
        {
            PrintStartupError(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            PrintStartupError(exception);
        }
        catch (ArgumentException exception)
        {
            PrintStartupError(exception);
        }
        catch (InvalidOperationException exception)
        {
            PrintStartupError(exception);
        }
        catch (KeyNotFoundException exception)
        {
            PrintStartupError(exception);
        }
    }

    private static void PrintStartupError(Exception exception)
    {
        System.Console.Error.WriteLine($"ОШИБКА ЗАПУСКА: {exception.Message}");
    }
}
