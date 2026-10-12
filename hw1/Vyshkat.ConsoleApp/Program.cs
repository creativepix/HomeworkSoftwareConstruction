using Microsoft.Extensions.DependencyInjection;
using Vyshkat.Domain.Interfaces;
using Vyshkat.Domain.Services;
using Vyshkat.Domain.Models;

var services = new ServiceCollection();
services.AddSingleton<IServiceCenter, ServiceCenter>();
services.AddSingleton<IInventoryChecker, InventoryNumberChecker>();
services.AddSingleton<IInventoryChecker, EasyLevelChecker>();
services.AddSingleton<IInventoryChecker, EnergyChecker>();
services.AddSingleton<Park>();

var provider = services.BuildServiceProvider();
var park = provider.GetRequiredService<Park>();

while (true)
{
    Console.WriteLine("""
                      1 - добавить транспорт
                      2 - добавить вещь
                      3 - отчет
                      4 - транспорт для новичков
                      5 - все инвентарные номера
                      0 - выход
                      Выбор: 
                      """);
    var command = Console.ReadLine();
    if (command == "0")
    {
        break;
    }
    else if (command == "1")
    {
        AddTransport(park);
    }
    else if (command == "2")
    {
        AddThing(park);
    }
    else if (command == "3")
    {
        ShowReport(park);
    }
    else if (command == "4")
    {
        ShowTransportForNewUsers(park);
    }
    else if (command == "5")
    {
        ShowAllItems(park);
    }
    else
    {
        Console.WriteLine("ОШИБКА: Неизвестная команда");
    }
}

static void AddTransport(Park park)
{
    Console.WriteLine("""
                      1 - самокат
                      2 - велосипед
                      3 - электровелосипед
                      """);
    var type = ReadText("Тип: ");
    var number = ReadText("Инвентарный номер (например: A01): ");
    var name = ReadText("Название: ");
    var easyLevel = ReadInt("Простота для новичка (1-10): ");

    ITransport transport;
    if (type == "1")
    {
        var energy = ReadDecimal("Расход энергии за день: ");
        transport = new Scooter(number, name, energy, easyLevel);
    }
    else if (type == "2")
    {
        transport = new Bicycle(number, name, easyLevel);
    }
    else if (type == "3")
    {
        var energy = ReadDecimal("Расход энергии за день: ");
        transport = new ElectricBicycle(number, name, energy, easyLevel);
    }
    else
    {
        Console.WriteLine("ОШИБКА: Такого транспорта нет в меню");
        return;
    }

    var result = park.AddTransport(transport);
    if (result == AddResult.Added)
    {
        Console.WriteLine("Транспорт принят");
    }
    else if (result == AddResult.DuplicateNumber)
    {
        Console.WriteLine("ОШИБКА: Такой инвентарный номер уже есть");
    }
    else if (result == AddResult.InvalidEasyLevel)
    {
        Console.WriteLine("ОШИБКА: Число должно быть от 1 до 10");
    }
    else if (result == AddResult.InvalidEnergy)
    {
        Console.WriteLine("ОШИБКА: Расход не может быть отрицательным");
    }
    else
    {
        Console.WriteLine("ОШИБКА: Техосмотр не пройден");
    }
}

static void AddThing(Park park)
{
    Console.WriteLine("""
                      1 - шлем
                      2 - док-станция
                      3 - зарядный шкаф
                      """);
    var type = ReadText("Тип: ");
    var number = ReadText("Инвентарный номер (например 01): ");

    if (type == "1")
    {
        var size = ReadText("Размер: ");
        var result = park.AddThing(new Helmet(number, size));
        PrintThingResult(result);
    }
    else if (type == "2")
    {
        var place = ReadText("Место: ");
        var result = park.AddThing(new DockStation(number, place));
        PrintThingResult(result);
    }
    else if (type == "3")
    {
        var place = ReadText("Место: ");
        var energy = ReadDecimal("Расход энергии за день: ");
        var result = park.AddThing(new ChargingCabinet(number, place, energy));
        PrintThingResult(result);
    }
    else
    {
        Console.WriteLine("ОШИБКА: Такой вещи нет в меню");
    }
}

static void PrintThingResult(AddResult result)
{
    if (result == AddResult.Added)
    {
        Console.WriteLine("Вещь добавлена");
    }
    else if (result == AddResult.DuplicateNumber)
    {
        Console.WriteLine("ОШИБКА: Такой инвентарный номер уже есть");
    }
    else if (result == AddResult.InvalidEnergy)
    {
        Console.WriteLine("ОШИБКА: Расход не может быть отрицательным");
    }
    else if (result == AddResult.InvalidEasyLevel)
    {
        Console.WriteLine("ОШИБКА: Число должно быть от 1 до 10");
    }
}

static void ShowReport(Park park)
{
    Console.WriteLine($"Всего на балансе: {park.TotalItemsCount()}");
    Console.WriteLine($"Суточное потребление: {park.TotalEnergyPerDay()} кВт*ч");
}

static void ShowTransportForNewUsers(Park park)
{
    foreach (var item in park.GetTransportForNewUsers())
    {
        Console.WriteLine($"{item.InventoryNumber}: {item.Name}, простота {item.EasyLevel}/10");
    }
}

static void ShowAllItems(Park park)
{
    foreach (var item in park.AllItems())
    {
        Console.WriteLine($"{item.InventoryNumber}: {item.Name}");
    }
}

static string ReadText(string text)
{
    Console.Write(text);
    return Console.ReadLine() ?? "";
}

static int ReadInt(string text)
{
    while (true)
    {
        var input = ReadText(text);
        if (int.TryParse(input, out var result))
        {
            return result;
        }

        Console.WriteLine("ОШИБКА: Введите целое число");
    }
}

static decimal ReadDecimal(string text)
{
    while (true)
    {
        var input = ReadText(text);
        if (decimal.TryParse(input, out var result))
        {
            return result;
        }

        Console.WriteLine("ОШИБКА: Введите число");
    }
}

