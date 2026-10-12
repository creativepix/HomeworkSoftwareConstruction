using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public class EasyLevelChecker : IInventoryChecker
{
    public AddResult Check(IInventoryItem item, IReadOnlyList<IInventoryItem> items)
    {
        if (item is IEasyToUse easyToUse && (easyToUse.EasyLevel < 1 || easyToUse.EasyLevel > 10))
        {
            return AddResult.InvalidEasyLevel;
        }

        return AddResult.Added;
    }
}
