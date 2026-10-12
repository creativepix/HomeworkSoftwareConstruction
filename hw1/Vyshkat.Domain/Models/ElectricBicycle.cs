using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Models;

public class ElectricBicycle : Transport, IEnergyConsumer
{
    public ElectricBicycle(string inventoryNumber, string model, decimal energyPerDay, int easyLevel)
        : base(inventoryNumber, $"Электровелосипед {model}", easyLevel)
    {
        EnergyPerDay = energyPerDay;
    }

    public decimal EnergyPerDay { get; }
}
