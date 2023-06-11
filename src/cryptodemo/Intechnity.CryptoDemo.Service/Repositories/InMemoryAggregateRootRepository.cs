using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.Repositories;

public class InMemoryAggregateRootRepository<TAggregate> : IAggregateRootRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    private readonly AggregateRootFactory _factory;

    private TAggregate? _instance;

    public InMemoryAggregateRootRepository(AggregateRootFactory factory)
    {
        _factory = factory;
    }

    public TAggregate Get()
    {
        if (_instance != null)
            return _instance;

        _instance = _factory.Create<TAggregate>();
        return _instance;
    }
}