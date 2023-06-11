using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Extensions;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.Services.P2P;

/// <summary>
/// The endpoints exposed by our node. Other nodes will communicate with us using this grpc service.
/// </summary>
public class P2PNodeEndpoint : P2PNode.P2PNodeBase
{
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IP2PManager _p2pManager;
    private readonly IDomainBus _domainBus;
    private readonly IMapper _mapper;

    public P2PNodeEndpoint(IAggregateRootRepository<Blockchain> blockchainRepository,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository,
        IP2PManager p2pManager,
        IDomainBus domainBus,
        IMapper mapper)
    {
        _blockchainRepository = blockchainRepository;
        _transactionPoolRepository = transactionPoolRepository;
        _p2pManager = p2pManager;
        _domainBus = domainBus;
        _mapper = mapper;
    }

    public override Task<ConnectMessage> Connect(ConnectMessage request, ServerCallContext context)
    {
        var portDelimiter = context.Peer.LastIndexOf(":");
        var peerListenAddress = context.Peer.Substring(0, portDelimiter + 1) + request.ListenPort;

        var targetSchema = "https://";
#if DEBUG
        targetSchema = "http://";
#endif
        peerListenAddress = peerListenAddress
            .Replace("ipv6:", targetSchema)
            .Replace("ipv4:", targetSchema);

        var allKnownPeers = request.KnownPeers.ToList();
        allKnownPeers.Add(peerListenAddress);

        _p2pManager.UpdateKnownPeers(context.Host, allKnownPeers);

        var response = new ConnectMessage();
        response.KnownPeers.AddRange(_p2pManager.KnownPeers);

        return Task.FromResult(response);
    }

    public override Task<SyncStatusResponse> SyncStatus(SyncStatusRequest request, ServerCallContext context)
    {
        var blockchain = _blockchainRepository.Get();
        var transactionPool = _transactionPoolRepository.Get();

        var protoUnconfirmedTransactions = _mapper.Map<List<ProtoTransaction>>(transactionPool.UnconfirmedTransactions);

        var response = new SyncStatusResponse();
        response.NoOfBlocks = blockchain.Blocks.Count;
        response.SumOfDifficulty = blockchain.Blocks.GetSumOfDifficulties();
        response.UnconfirmedTransactions.AddRange(protoUnconfirmedTransactions);
        response.KnownPeers.AddRange(_p2pManager.KnownPeers);

        _p2pManager.UpdateKnownPeers(context.Host, request.KnownPeers);

        return Task.FromResult(response);
    }

    public override Task<DownloadBlocksResponse> DownloadBlocks(DownloadBlocksRequest request, ServerCallContext context)
    {
        var blockchain = _blockchainRepository.Get();
        var blocks = blockchain.Blocks.Where(x => x.Index >= request.FromBlock && x.Index <= request.ToBlock).ToList();

        var protoBlocks = _mapper.Map<List<ProtoBlock>>(blocks);
        var response = new DownloadBlocksResponse();
        response.Blocks.AddRange(protoBlocks);

        return Task.FromResult(response);
    }

    public override async Task<Empty> OnNewTransaction(OnNewTransactionRequest request, ServerCallContext context)
    {
        List<ProtoTransaction> transactionDTOs = _mapper.Map<List<ProtoTransaction>>(request.Transactions);
        var domainEvent = new NewTransactionDistributedNotification(transactionDTOs);
        await _domainBus.QueueEvent(domainEvent);

        return new Empty();
    }

    public override async Task<Empty> OnNewBlockMinted(OnNewBlockMintedRequest request, ServerCallContext context)
    {
        var domainEvent = new NewBlockMinedDistributedNotification(request.NewBlock);
        await _domainBus.QueueEvent(domainEvent);

        return new Empty();
    }
}