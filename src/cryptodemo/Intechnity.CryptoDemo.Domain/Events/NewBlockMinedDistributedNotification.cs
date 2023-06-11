using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record NewBlockMinedDistributedNotification : IDomainNotification
{
    public ProtoBlock Block { get; init; }

    public NewBlockMinedDistributedNotification(ProtoBlock block)
    {
        Block = block;
    }
}