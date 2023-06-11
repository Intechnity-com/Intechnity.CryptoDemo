namespace Intechnity.CryptoDemo.Domain.Framework;

public interface IApply<TEvent>
    where TEvent : class
{
    void Apply(TEvent @event);
}