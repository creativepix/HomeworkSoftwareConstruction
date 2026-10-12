using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public class ServiceCenter : IServiceCenter
{
    public bool CanAccept(ITransport transport)
    {
        if (transport is IEnergyConsumer energyConsumer)
        {
            return energyConsumer.EnergyPerDay > 0;
        }

        return true;
    }
}