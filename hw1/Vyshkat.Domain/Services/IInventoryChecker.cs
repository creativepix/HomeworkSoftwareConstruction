using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public interface IInventoryChecker
{
    AddResult Check(IInventoryItem item, IReadOnlyList<IInventoryItem> items);
}