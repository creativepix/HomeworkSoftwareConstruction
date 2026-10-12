using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Models;

public abstract class InventoryItem : IInventoryItem
{
    protected InventoryItem(string inventoryNumber, string name)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
        {
            throw new ArgumentException("Номер инвенатря не должен быть пустым", nameof(inventoryNumber));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название не должно быть пустым", nameof(name));
        }

        InventoryNumber = inventoryNumber;
        Name = name;
    }

    public string InventoryNumber { get; }
    public string Name { get; }
}