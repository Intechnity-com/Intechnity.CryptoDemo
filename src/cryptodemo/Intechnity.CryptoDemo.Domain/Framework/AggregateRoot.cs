using AutoMapper;
using Intechnity.CryptoDemo.Core.Providers;

namespace Intechnity.CryptoDemo.Domain.Framework;

public abstract class AggregateRoot
{
    private readonly IDomainBus _bus;
    protected readonly IMapper Mapper;
    protected readonly IDateTimeProvider DateTimeProvider;

    public AggregateRoot(IDomainBus bus, 
        IMapper mapper, 
        IDateTimeProvider dateTimeProvider)
    {
        _bus = bus;
        Mapper = mapper;
        DateTimeProvider = dateTimeProvider;
    }

    protected virtual async Task RaiseEvent(IDomainNotification @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        ApplyEvent(@event);
        await _bus.RaiseImmediately(@event).ConfigureAwait(false);
    }

    private void ApplyEvent(IDomainNotification @event)
    {
        var applyType = typeof(IApply<>).MakeGenericType(@event.GetType());
        if (!applyType.IsAssignableFrom(this.GetType()))
        {
            throw new NotImplementedException($"Missing IApply implementation for {@event.GetType().FullName}.");
        }

        applyType.GetMethod(nameof(IApply<object>.Apply)).Invoke(this, new [] { @event });
    }
}