using AutoMapper;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Intechnity.CryptoDemo.Core.Models.Configuration;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Service.Services.P2P;

public class P2PManager : IP2PManager, IDisposable
{
    private const int CONNECT_TIMEOUT_MS = 5000;

    private readonly IDomainBus _domainBus;
    private readonly IOptions<BlockchainConfiguration> _blockchainConfiguration;
    private readonly ILogger<P2PManager> _logger;
    private readonly IMapper _mapper;
    private readonly IServerInfoProvider _serverInfoProvider;

    private readonly ConcurrentDictionary<string, GrpcChannel?> _knownPeers = new ConcurrentDictionary<string, GrpcChannel?>();
    private bool _isLoaded = false;

    public bool IsReady => _isLoaded;

    public IReadOnlyList<string> KnownPeers => _knownPeers.Keys.ToList().AsReadOnly();

    public P2PManager(
        IDomainBus domainBus,
        IOptions<BlockchainConfiguration> blockchainConfiguration,
        ILogger<P2PManager> logger,
        IMapper mapper,
        IServerInfoProvider serverInfoProvider)
    {
        _domainBus = domainBus;
        _blockchainConfiguration = blockchainConfiguration;
        _logger = logger;
        _mapper = mapper;
        _serverInfoProvider = serverInfoProvider;
    }

    public async Task LoadInitialBlockchainAsync(CancellationToken cancellationToken)
    {
        UpdateKnownPeers(null, _blockchainConfiguration.Value.KnownIpAddresses);

        await _domainBus.RaiseImmediately(new LocalBlockchainStateChangedNotification()); // raise initial status change
        await SyncBlockchainStateWithKnownPeersAsync(cancellationToken);

        _isLoaded = true;
    }

    public async Task SyncBlockchainStateWithKnownPeersAsync(CancellationToken cancellationToken)
    {
        var request = new SyncStatusRequest();
        request.KnownPeers.AddRange(KnownPeers);

        var peerStatusResponses = await GetDataFromPeers(client => client.SyncStatus(request));
        var @event = new GotBlockchainSyncResponsesNotification(peerStatusResponses);
        await _domainBus.RaiseImmediately(@event);
    }

    public async Task DistributeNewTransactionToKnownPeersAsync(UserCreatedNewTransactionNotification userCreatedNewTransactionEvent)
    {
        var request = new OnNewTransactionRequest();
        request.Transactions.AddRange(_mapper.Map<List<ProtoTransaction>>(userCreatedNewTransactionEvent.TransactionDTOs));

        await InvokeEventOnPeers(client => client.OnNewTransaction(request));
    }

    public async Task DistributeNewMinedBlockToKnownPeersAsync(AppendMinedBlockToBlockchainNotification userMinedNewBlockEvent)
    {
        var request = new OnNewBlockMintedRequest();
        request.NewBlock = _mapper.Map<ProtoBlock>(userMinedNewBlockEvent.Block);

        await InvokeEventOnPeers(client => client.OnNewBlockMinted(request));
    }

    public void UpdateKnownPeers(string? host, IEnumerable<string> peers)
    {
        foreach (var peer in peers)
        {
            if (_knownPeers.ContainsKey(peer))
                continue;

            if (host != null && host == peer) 
                continue;

            if (!_knownPeers.ContainsKey(peer))
                _knownPeers.TryAdd(peer, null);
        }
    }

    private async Task InvokeEventOnPeers(Action<P2PNode.P2PNodeClient> action)
    {
        var tasks = new List<Task>();
        foreach (var peer in _knownPeers.Keys)
        {
            var channel = _knownPeers.GetValueOrDefault(peer);
            if (channel == null)
            {
                channel = await CreateGrpcChannelAndTryConnect(peer);
                if (channel == null)
                    DeleteKnownPeer(peer);
                else
                    _knownPeers[peer] = channel;
            }

            if (channel != null)
            {
                var task = InvokeEventOnPeer(peer, channel, action);
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }

    private Task InvokeEventOnPeer(string peer, GrpcChannel channel, Action<P2PNode.P2PNodeClient> action)
    {
        try
        {
            var client = new P2PNode.P2PNodeClient(channel);
            return Task.Run(() => action.Invoke(client));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call peer");
            DeleteKnownPeer(peer);
        }

        return Task.CompletedTask;
    }

    private async Task<Dictionary<string, TResult>> GetDataFromPeers<TResult>(Func<P2PNode.P2PNodeClient, TResult> action)
    {
        var tasks = new List<Task>();
        var result = new Dictionary<string, TResult>();

        async Task Action(string peer)
        {
            var data = await GetDataFromPeer(peer, action);
            if (data != null)
            {
                result!.Add(peer, data);
            }
        }

        foreach (var peer in _knownPeers.Keys)
        {
            var task = Action(peer);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return result;
    }

    public async Task<TResult?> GetDataFromPeer<TResult>(string peer, Func<P2PNode.P2PNodeClient, TResult> action)
    {
        var channel = _knownPeers.GetValueOrDefault(peer);
        if (channel == null)
        {
            channel = await CreateGrpcChannelAndTryConnect(peer);
            if (channel == null)
                DeleteKnownPeer(peer);
            else
                _knownPeers[peer] = channel;
        }

        if (channel == null)
            return default(TResult?);

        return await GetDataFromPeer(peer, channel, action);
    }

    private Task<TResult?> GetDataFromPeer<TResult>(string peer, GrpcChannel channel, Func<P2PNode.P2PNodeClient, TResult> action)
    {
        try
        {
            var client = new P2PNode.P2PNodeClient(channel);
            var task = Task.Run(() => action.Invoke(client));

            return task!;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call peer " + peer);
            DeleteKnownPeer(peer);

            return Task.FromResult(default(TResult));
        }
    }

    private void DeleteKnownPeer(string peer)
    {
        var channel = _knownPeers.GetValueOrDefault(peer);
        channel?.Dispose();

        _knownPeers.TryRemove(peer, out _);
    }

    private async Task<GrpcChannel?> CreateGrpcChannelAndTryConnect(string peer)
    {
        try
        {
            var httpHandler = new HttpClientHandler
            {
#if DEBUG
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
            };

            var channel = GrpcChannel.ForAddress(peer, new GrpcChannelOptions { HttpHandler = httpHandler });

            var connectMessage = new ConnectMessage();
            connectMessage.KnownPeers.Add(_knownPeers.Keys);
            connectMessage.ListenPort = _serverInfoProvider.GetListenPort();

            var client = new P2PNode.P2PNodeClient(channel);
            await client.ConnectAsync(connectMessage, deadline: DateTime.UtcNow.AddMilliseconds(CONNECT_TIMEOUT_MS));

            return channel;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void Dispose()
    {
        foreach (var peer in _knownPeers.Keys.ToList())
        {
            DeleteKnownPeer(peer);
        }
    }
}