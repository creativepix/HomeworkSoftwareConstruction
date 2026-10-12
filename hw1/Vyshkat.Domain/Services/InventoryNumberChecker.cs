using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public class InventoryNumberChecker : IInventoryChecker
{
    public AddResult Check(IInventoryItem item, IReadOnlyList<IInventoryItem> items)
    {
        foreach (var oldItem in items)
        {
            if (oldItem.InventoryNumber == item.InventoryNumber)
            {
                return AddResult.DuplicateNumber;
            }
        }

        return AddResult.Added;
    }
}