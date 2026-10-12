namespace Vyshkat.Domain.Interfaces;

public interface IServiceCenter
{
    bool CanAccept(ITransport transport);
}