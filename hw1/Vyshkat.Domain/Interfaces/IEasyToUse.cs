namespace Vyshkat.Domain.Interfaces;

public interface IEasyToUse
{
    int EasyLevel { get; }
    bool IsGoodForNewUser { get; }
}