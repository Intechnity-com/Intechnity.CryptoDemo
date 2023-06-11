using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record GotBlockchainSyncResponsesNotification : IDomainNotification
{
    public Dictionary<string, SyncStatusResponse> PeerSyncStatusResponses { get; init; }

    public GotBlockchainSyncResponsesNotification(Dictionary<string, SyncStatusResponse> syncStatusResponses)
    {
        PeerSyncStatusResponses = syncStatusResponses;
    }
}