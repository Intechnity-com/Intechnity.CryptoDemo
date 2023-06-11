using Microsoft.Extensions.DependencyInjection;

namespace Intechnity.CryptoDemo.Domain.Framework;

public class AggregateRootFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AggregateRootFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TAggregate Create<TAggregate>()
        where TAggregate : AggregateRoot
            => (TAggregate)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(TAggregate));
}