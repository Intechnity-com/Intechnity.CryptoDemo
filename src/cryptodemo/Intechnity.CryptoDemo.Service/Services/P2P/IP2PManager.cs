using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Service.Services.P2P;

public interface IP2PManager
{
    bool IsReady { get; }

    IReadOnlyList<string> KnownPeers { get; }

    Task LoadInitialBlockchainAsync(CancellationToken cancellationToken);

    Task SyncBlockchainStateWithKnownPeersAsync(CancellationToken cancellationToken);

    Task DistributeNewTransactionToKnownPeersAsync(UserCreatedNewTransactionNotification request);

    Task DistributeNewMinedBlockToKnownPeersAsync(AppendMinedBlockToBlockchainNotification blockMinedEvent);

    Task<TResult?> GetDataFromPeer<TResult>(string peer, Func<P2PNode.P2PNodeClient, TResult> action);

    void UpdateKnownPeers(string? host, IEnumerable<string> allKnownPeers);
}