using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public class EnergyChecker : IInventoryChecker
{
    public AddResult Check(IInventoryItem item, IReadOnlyList<IInventoryItem> items)
    {
        if (item is IEnergyConsumer energyConsumer && energyConsumer.EnergyPerDay < 0)
        {
            return AddResult.InvalidEnergy;
        }

        return AddResult.Added;
    }
}
