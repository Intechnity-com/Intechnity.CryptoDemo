using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Helpers;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Service.BackgroundServices;

public class MintingHostedService : IHostedService
{
    private const int MINT_INTERVAL_SEC = 1;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAppState _appState;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

    public MintingHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IAppState appState,
        IDateTimeProvider dateTimeProvider,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _appState = appState;
        _dateTimeProvider = dateTimeProvider;
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            Task.Factory.StartNew(() => StartInternal(cancellationToken));

        return Task.CompletedTask;
    }

    private async void StartInternal(CancellationToken externalCancellationToken)
    {
        while (!_cancellationTokenSource.IsCancellationRequested && !externalCancellationToken.IsCancellationRequested)
        {
            if (_appState.IsMinting)
                await TryToFindBlock();

            await Task.Delay(TimeSpan.FromSeconds(MINT_INTERVAL_SEC));
        }
    }

    private async Task TryToFindBlock()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var transactionPoolRepository = scope.ServiceProvider.GetRequiredService<IAggregateRootRepository<TransactionPool>>();
            var blockchainRepository = scope.ServiceProvider.GetRequiredService<IAggregateRootRepository<Blockchain>>();

            var transactionPool = transactionPoolRepository.Get();
            var blockchain = blockchainRepository.Get();

            var mintingAddress = _appState.MintingAccount!.PublicPrivateKeyPair.PublicKey;

            if (transactionPool.UnconfirmedTransactions.Count == 0)
                return;

            var timestamp = DateTimeOffset.Now;
            var transactionsToInclude = new List<Transaction>
            {
                TransactionsHelper.CreateCoinbaseTransaction(mintingAddress) // here we reward ourselves for finding the block (if we succeed of course)
            };
            transactionsToInclude.AddRange(transactionPool.UnconfirmedTransactions.Values);

            var lastBlock = blockchain.Blocks.Last();

            var block = new Block(
                CryptoDemoConsts.BLOCKCHAIN_ID,
                CryptoDemoConsts.BLOCKCHAIN_VERSION,
                lastBlock.Index + 1,
                timestamp,
                transactionsToInclude,
                CryptoDemoConsts.MINTING_DIFFICULTY,
                mintingAddress,
                lastBlock.Hash);

            if (blockchain.ValidateBlock(block, blockchain.Blocks, transactionPool, _publicPrivateKeyGenerator))
                await blockchain.ValidateAndAppendMyMinedBlock(block, transactionPool, _publicPrivateKeyGenerator, _dateTimeProvider);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();

        return Task.CompletedTask;
    }
}