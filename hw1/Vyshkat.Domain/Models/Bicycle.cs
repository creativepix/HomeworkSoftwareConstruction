namespace Vyshkat.Domain.Models;

public class Bicycle : Transport
{
    public Bicycle(string inventoryNumber, string model, int easyLevel)
        : base(inventoryNumber, $"Велосипед {model}", easyLevel)
    {
    }
}