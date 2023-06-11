using MediatR;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class AppendMinedBlockToBlockchainNotificationHandler : INotificationHandler<AppendMinedBlockToBlockchainNotification>
{
    private readonly IP2PManager _p2pManager;
    private readonly IDomainBus _domainBus;

    public AppendMinedBlockToBlockchainNotificationHandler(
        IP2PManager p2pManager,
        IDomainBus domainBus)
    {
        _p2pManager = p2pManager;
        _domainBus = domainBus;
    }

    public async Task Handle(AppendMinedBlockToBlockchainNotification request, CancellationToken cancellationToken)
    {
        var notification = new LocalBlockchainStateChangedNotification();
        await _domainBus.RaiseImmediately(notification);

        if (request.DistributeToPeers)
            await _p2pManager.DistributeNewMinedBlockToKnownPeersAsync(request);
    }
}