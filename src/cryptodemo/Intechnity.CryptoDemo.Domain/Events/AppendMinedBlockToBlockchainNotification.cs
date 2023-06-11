using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record AppendMinedBlockToBlockchainNotification : IDomainNotification
{
    public ProtoBlock Block { get; init; }

    public bool DistributeToPeers { get; init; }

    public AppendMinedBlockToBlockchainNotification(ProtoBlock block, bool distributeToPeers)
    {
        Block = block;
        DistributeToPeers = distributeToPeers;
    }
}