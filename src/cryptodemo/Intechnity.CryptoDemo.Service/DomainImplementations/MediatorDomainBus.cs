using MediatR;
using Intechnity.CryptoDemo.Domain.Framework;

namespace Intechnity.CryptoDemo.Service.DomainImplementations;

public class MediatorDomainBus : IDomainBus
{
    private readonly IMediator _mediator;

    public MediatorDomainBus(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task RaiseImmediately<TEvent>(TEvent @event)
        where TEvent : IDomainNotification
    {
        await _mediator.Publish(@event);
    }

    public async Task QueueEvent<TEvent>(TEvent @event)
        where TEvent : IDomainNotification
    {
        await _mediator.Publish(@event); // will be replaced in future to just queue event without awaiting for result
    }
}