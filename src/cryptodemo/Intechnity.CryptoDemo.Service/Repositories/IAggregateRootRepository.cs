using Intechnity.CryptoDemo.Domain.Framework;

namespace Intechnity.CryptoDemo.Service.Framework;

public interface IAggregateRootRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    TAggregate Get();
}