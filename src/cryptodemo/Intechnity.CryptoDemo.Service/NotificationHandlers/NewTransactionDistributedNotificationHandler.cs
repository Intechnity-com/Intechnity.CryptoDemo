using MediatR;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class NewTransactionDistributedNotificationHandler : INotificationHandler<NewTransactionDistributedNotification>
{
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public NewTransactionDistributedNotificationHandler(IAggregateRootRepository<TransactionPool> transactionPoolRepository, 
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _transactionPoolRepository = transactionPoolRepository;
        _blockchainRepository = blockchainRepository;
    }

    public Task Handle(NewTransactionDistributedNotification request, CancellationToken cancellationToken)
    {
        var blockchain = _blockchainRepository.Get();

        var transactionPool = _transactionPoolRepository.Get();
        transactionPool.Apply(blockchain.Blocks, request);

        return Task.CompletedTask;
    }
}