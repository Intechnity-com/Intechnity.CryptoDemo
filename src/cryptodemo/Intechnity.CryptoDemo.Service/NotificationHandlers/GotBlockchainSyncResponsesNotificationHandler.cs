using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Extensions;
using Intechnity.CryptoDemo.Domain.Protos;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class GotBlockchainSyncResponsesNotificationHandler : INotificationHandler<GotBlockchainSyncResponsesNotification>
{
    private readonly ILogger<GotBlockchainSyncResponsesNotificationHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;
    private readonly IP2PManager _p2pManager;
    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

    public GotBlockchainSyncResponsesNotificationHandler(
        ILogger<GotBlockchainSyncResponsesNotificationHandler> logger,
        IMapper mapper,
        IAggregateRootRepository<Blockchain> blockchainRepository,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository,
        IAggregateRootRepository<Wallet> walletRepository,
        IP2PManager p2pManager,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        _logger = logger;
        _mapper = mapper;
        _blockchainRepository = blockchainRepository;
        _transactionPoolRepository = transactionPoolRepository;
        _walletRepository = walletRepository;
        _p2pManager = p2pManager;
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
    }

    public async Task Handle(GotBlockchainSyncResponsesNotification request, CancellationToken cancellationToken)
    {
        var myBlockchain = _blockchainRepository.Get();
        var myTransactionPool = _transactionPoolRepository.Get();
        var myWallet = _walletRepository.Get();
        var myBlocksDifficulty = myBlockchain.Blocks.GetSumOfDifficulties();

        foreach (var response in request.PeerSyncStatusResponses)
        {
            var peer = response.Key;
            var peerResponse = response.Value;
            var peerUnconfirmedTransactionDTOs = _mapper.Map<List<ProtoTransaction>>(peerResponse.UnconfirmedTransactions);

            if (peerResponse.SumOfDifficulty > myBlocksDifficulty)
            {
                var downloadBlocksRequest = new DownloadBlocksRequest();
                downloadBlocksRequest.FromBlock = Math.Max(1, myBlockchain.Blocks.Count - CryptoDemoConsts.SYNC_STATUS_BLOCKS_MARGIN);
                downloadBlocksRequest.ToBlock = peerResponse.NoOfBlocks;

                var peerBlocksResult = await _p2pManager.GetDataFromPeer(peer, client => client.DownloadBlocks(downloadBlocksRequest));
                if (peerBlocksResult == null)
                {
                    continue;
                }

                try
                {
                    var blocksDTO = _mapper.Map<List<ProtoBlock>>(peerBlocksResult.Blocks);
                    var blocks = _mapper.Map<List<Block>>(blocksDTO);
                    await myBlockchain.ValidateAndSetBlocksFromBlockchainSync(blocks, myTransactionPool, _publicPrivateKeyGenerator);

                    myBlocksDifficulty = myBlockchain.Blocks.GetSumOfDifficulties();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not download data from peer");
                }
            }

            myTransactionPool.AddUnconfirmedTransactions(myBlockchain.Blocks, peerUnconfirmedTransactionDTOs);
        }
    }
}