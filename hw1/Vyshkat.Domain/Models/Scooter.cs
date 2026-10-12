using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Models;

public class Scooter : Transport, IEnergyConsumer
{
    public Scooter(string inventoryNumber, string model, decimal energyPerDay, int easyLevel)
        : base(inventoryNumber, $"Самокат {model}", easyLevel)
    {
        EnergyPerDay = energyPerDay;
    }

    public decimal EnergyPerDay { get; }
}
