using Vyshkat.Domain.Interfaces;
using Vyshkat.Domain.Models;
using Vyshkat.Domain.Services;

namespace Vyshkat.Tests;

public class ParkTests
{
    private class FakeServiceCenter : IServiceCenter
    {
        private readonly bool _result;

        public FakeServiceCenter(bool result)
        {
            _result = result;
        }

        public bool CanAccept(ITransport transport)
        {
            return _result;
        }
    }

    private static Park CreatePark(bool serviceCenterResult)
    {
        return new Park(new FakeServiceCenter(serviceCenterResult),
            new List<IInventoryChecker>
            {
                new InventoryNumberChecker(),
                new EasyLevelChecker(),
                new EnergyChecker()
            });
    }

    [Fact]
    public void TransportIsAddedAfterGoodCheck()
    {
        var park = CreatePark(true);
        var scooter = new Scooter("01", "обычный", 1.5m, 8);
        var result = park.AddTransport(scooter);

        Assert.Equal(AddResult.Added, result);
        Assert.Contains(scooter, park.Transport);
    }

    [Fact]
    public void EnergyReportIncludesTransportThings()
    {
        var park = CreatePark(true);
        park.AddTransport(new Scooter("01", "обычный", 1.5m, 8));
        park.AddTransport(new Bicycle("02", "простой", 7));
        park.AddThing(new ChargingCabinet("03", "корпус", 3.8m));
        park.AddThing(new Helmet("04", "M"));

        Assert.Equal(5.3m, park.TotalEnergyPerDay());
    }

    [Fact]
    public void TransportIsNotAddedAfterBadCheck()
    {
        var park = CreatePark(false);
        var scooter = new Scooter("02", "сломанный", 1.5m, 8);
        var result = park.AddTransport(scooter);

        Assert.Equal(AddResult.InspectionFailed, result);
        Assert.Empty(park.Transport);
    }

    [Fact]
    public void SameInventoryNumberIsRejected()
    {
        var park = CreatePark(true);
        park.AddTransport(new Scooter("01", "обычный", 1.5m, 8));
        var sameTransportNumber = park.AddTransport(new Bicycle("01", "простой", 7));
        var sameThingNumber = park.AddThing(new Helmet("01", "M"));

        Assert.Equal(AddResult.DuplicateNumber, sameTransportNumber);
        Assert.Equal(AddResult.DuplicateNumber, sameThingNumber);
        Assert.Single(park.AllItems());
    }

    [Fact]
    public void NewUserListUsesLevel()
    {
        var park = CreatePark(true);
        var easyScooter = new Scooter("01", "обычный", 1.5m, 8);
        var hardScooter = new Scooter("02", "быстрый", 2.5m, 4);
        var bicycle = new Bicycle("03", "простой", 6);

        park.AddTransport(easyScooter);
        park.AddTransport(hardScooter);
        park.AddTransport(bicycle);

        var result = park.GetTransportForNewUsers();
        Assert.Contains(easyScooter, result);
        Assert.Contains(bicycle, result);
        Assert.DoesNotContain(hardScooter, result);
    }

    [Fact]
    public void InventoryListContainsTransportThings()
    {
        var park = CreatePark(true);
        park.AddTransport(new Scooter("01", "обычный", 1.5m, 8));
        park.AddThing(new Helmet("02", "M"));

        var inventoryNumbers = new List<string>();
        foreach (var item in park.AllItems())
        {
            inventoryNumbers.Add(item.InventoryNumber);
        }

        Assert.Equal(new List<string> { "01", "02" }, inventoryNumbers);
    }

    [Fact]
    public void EasyLevelCheckerChecksTaskRange()
    {
        var park = CreatePark(true);

        var tooSmall = park.AddTransport(new Scooter("01", "обычный", 1.5m, 0));
        var correct = park.AddTransport(new Scooter("02", "обычный", 1.5m, 1));
        var tooBig = park.AddTransport(new Scooter("03", "обычный", 1.5m, 11));

        Assert.Equal(AddResult.InvalidEasyLevel, tooSmall);
        Assert.Equal(AddResult.Added, correct);
        Assert.Equal(AddResult.InvalidEasyLevel, tooBig);
    }

    [Fact]
    public void EnergyCheckerRejectsNegativeValue()
    {
        var park = CreatePark(true);

        var negativeTransportEnergy = park.AddTransport(new Scooter("01", "обычный", -1, 8));
        var negativeThingEnergy = park.AddThing(new ChargingCabinet("02", "корпус", -1));
        var correct = park.AddThing(new ChargingCabinet("03", "корпус", 0));

        Assert.Equal(AddResult.InvalidEnergy, negativeTransportEnergy);
        Assert.Equal(AddResult.InvalidEnergy, negativeThingEnergy);
        Assert.Equal(AddResult.Added, correct);
    }
}
