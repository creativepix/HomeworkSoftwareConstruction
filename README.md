# HomeworkSoftwareConstruction

Здесь хранятся домашние задания по курсу "Конструирование программного обеспечения".

## Структура

- `hwX` - папка для домашнего задания с номером `X`.

## Как запустить домашку

Перейти в папку нужной домашки (`X` -номер домашки):

```bash
cd hwX
```

Собрать проект:

```bash
dotnet build
```

Запустить приложение. Для `hw1`:

```bash
dotnet run --project Vyshkat.ConsoleApp/Vyshkat.ConsoleApp.csproj
```

Запустить тесты:

```bash
dotnet test
```
