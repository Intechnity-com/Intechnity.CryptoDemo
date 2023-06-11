namespace Intechnity.CryptoDemo.Domain.Framework;

public interface IDomainBus
{
    Task RaiseImmediately<TEvent>(TEvent @event)
        where TEvent : IDomainNotification;

    Task QueueEvent<TEvent>(TEvent @event)
        where TEvent : IDomainNotification;
}