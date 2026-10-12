using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Models;

public class ChargingCabinet : InventoryItem, IEnergyConsumer
{
    public ChargingCabinet(string inventoryNumber, string place, decimal energyPerDay)
        : base(inventoryNumber, $"Зарядный шкаф: {place}")
    {
        EnergyPerDay = energyPerDay;
    }

    public decimal EnergyPerDay { get; }
}
