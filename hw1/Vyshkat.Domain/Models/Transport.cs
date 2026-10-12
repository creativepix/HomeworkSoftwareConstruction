using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Models;

public abstract class Transport : InventoryItem, ITransport
{
    protected Transport(string inventoryNumber, string name, int easyLevel) : base(inventoryNumber, name)
    {
        EasyLevel = easyLevel;
    }

    public int EasyLevel { get; }
    public bool IsGoodForNewUser => EasyLevel >= 6;
}
