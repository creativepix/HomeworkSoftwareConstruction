namespace Vyshkat.Domain.Models;

public class Helmet : InventoryItem
{
    public Helmet(string inventoryNumber, string size) : base(inventoryNumber, $"Шлем {size}")
    {
    }
}