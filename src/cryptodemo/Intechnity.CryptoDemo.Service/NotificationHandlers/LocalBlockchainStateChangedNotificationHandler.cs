using MediatR;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class LocalBlockchainStateChangedNotificationHandler : INotificationHandler<LocalBlockchainStateChangedNotification>
{
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;

    public LocalBlockchainStateChangedNotificationHandler(
        IAggregateRootRepository<Blockchain> blockchainRepository,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository,
        IAggregateRootRepository<Wallet> walletRepository)
    {
        _blockchainRepository = blockchainRepository;
        _transactionPoolRepository = transactionPoolRepository;
        _walletRepository = walletRepository;
    }

    public Task Handle(LocalBlockchainStateChangedNotification request, CancellationToken cancellationToken)
    {
        var myBlockchain = _blockchainRepository.Get();
        var myTransactionPool = _transactionPoolRepository.Get();
        var myWallet = _walletRepository.Get();

        myTransactionPool.ProcessAllTransactions(myBlockchain.Blocks);
        myWallet.UpdateWalletBalances(myTransactionPool);

        return Task.FromResult(Unit.Value);
    }
}