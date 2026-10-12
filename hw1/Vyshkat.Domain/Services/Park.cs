using Vyshkat.Domain.Interfaces;

namespace Vyshkat.Domain.Services;

public class Park
{
    private readonly IServiceCenter _serviceCenter;
    private readonly IReadOnlyList<IInventoryChecker> _checkers;
    private readonly List<ITransport> _transport = new List<ITransport>();
    private readonly List<IInventoryItem> _things = new List<IInventoryItem>();

    public Park(IServiceCenter serviceCenter, IEnumerable<IInventoryChecker> checkers)
    {
        _serviceCenter = serviceCenter;
        _checkers = checkers.ToList();
    }

    public IReadOnlyList<ITransport> Transport => _transport;
    public IReadOnlyList<IInventoryItem> Things => _things;

    public AddResult AddTransport(ITransport transport)
    {
        var checkResult = CheckItem(transport);
        if (checkResult != AddResult.Added)
        {
            return checkResult;
        }

        if (!_serviceCenter.CanAccept(transport))
        {
            return AddResult.InspectionFailed;
        }

        _transport.Add(transport);
        return AddResult.Added;
    }

    public AddResult AddThing(IInventoryItem thing)
    {
        var checkResult = CheckItem(thing);
        if (checkResult != AddResult.Added)
        {
            return checkResult;
        }

        _things.Add(thing);
        return AddResult.Added;
    }

    public int TotalItemsCount()
    {
        return _transport.Count + _things.Count;
    }

    public decimal TotalEnergyPerDay()
    {
        decimal result = 0;

        foreach (var item in _transport)
        {
            if (item is IEnergyConsumer energyConsumer)
            {
                result += energyConsumer.EnergyPerDay;
            }
        }

        foreach (var item in _things)
        {
            if (item is IEnergyConsumer energyConsumer)
            {
                result += energyConsumer.EnergyPerDay;
            }
        }

        return result;
    }

    public IReadOnlyList<ITransport> GetTransportForNewUsers()
    {
        var result = new List<ITransport>();
        foreach (var item in _transport)
        {
            if (item.IsGoodForNewUser)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public IReadOnlyList<IInventoryItem> AllItems()
    {
        var result = new List<IInventoryItem>();
        foreach (var item in _transport)
        {
            result.Add(item);
        }

        result.AddRange(_things);
        return result;
    }

    private AddResult CheckItem(IInventoryItem item)
    {
        foreach (var checker in _checkers)
        {
            var result = checker.Check(item, AllItems());
            if (result != AddResult.Added)
            {
                return result;
            }
        }

        return AddResult.Added;
    }
}