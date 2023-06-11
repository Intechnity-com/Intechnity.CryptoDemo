using MediatR;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class UserCreatedNewTransactionNotificationHandler : INotificationHandler<UserCreatedNewTransactionNotification>
{
    private readonly IP2PManager _p2pManager;
    private readonly IDomainBus _domainBus;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public UserCreatedNewTransactionNotificationHandler(
        IP2PManager p2pMessagesBus,
        IDomainBus domainBus,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository,
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _p2pManager = p2pMessagesBus;
        _domainBus = domainBus;
        _transactionPoolRepository = transactionPoolRepository;
        _blockchainRepository = blockchainRepository;
    }

    public async Task Handle(UserCreatedNewTransactionNotification request, CancellationToken cancellationToken)
    {
        var blockchain = _blockchainRepository.Get();
        
        var transactionPool = _transactionPoolRepository.Get();
        transactionPool.Apply(blockchain.Blocks, request);

        var notification = new LocalBlockchainStateChangedNotification();
        await _domainBus.RaiseImmediately(notification);

        await _p2pManager.DistributeNewTransactionToKnownPeersAsync(request);
    }
}