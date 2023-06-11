using MediatR;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class ApplySyncedBlocksEventNotificationHandler : INotificationHandler<ApplySyncedBlocksNotification>
{
    private readonly IDomainBus _domainBus;

    public ApplySyncedBlocksEventNotificationHandler(IDomainBus domainBus)
    {
        _domainBus = domainBus;
    }

    public async Task Handle(ApplySyncedBlocksNotification request, CancellationToken cancellationToken)
    {
        var notification = new LocalBlockchainStateChangedNotification();
        await _domainBus.RaiseImmediately(notification);
    }
}