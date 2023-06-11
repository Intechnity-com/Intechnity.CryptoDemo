using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record ApplySyncedBlocksNotification : IDomainNotification
{
    public IReadOnlyList<ProtoBlock> SyncedBlocks { get; init; }

    public ApplySyncedBlocksNotification(IReadOnlyList<ProtoBlock> syncedBlocks)
    {
        SyncedBlocks = syncedBlocks;
    }
}