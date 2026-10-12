namespace Vyshkat.Domain.Models;

public class DockStation : InventoryItem
{
    public DockStation(string inventoryNumber, string place) : base(inventoryNumber, $"Док-станция: {place}")
    {
    }
}