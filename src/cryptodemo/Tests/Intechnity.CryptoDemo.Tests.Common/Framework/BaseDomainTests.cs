using AutoMapper;
using Moq;
using Intechnity.CryptoDemo.Console.Bootstrap;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services;
using Intechnity.CryptoDemo.Tests.Common.Fixtures;
using Intechnity.CryptoDemo.Tests.Common.Providers;

namespace Intechnity.CryptoDemo.Tests.Common.Framework;

public class BaseDomainTests
{
    protected readonly IMapper Mapper;

    protected readonly IDomainBus DomainBus;

    protected readonly IDataProtector DataProtectorMock;

    protected readonly IPublicPrivateKeyGenerator PublicPrivateKeyGenerator;

    protected readonly IBlockchainSeed BlockchainSeed;

    protected readonly MockDateTimeProvider DateTimeProvider;

    public BaseDomainTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        Mapper = new Mapper(configuration);
        DomainBus = Mock.Of<IDomainBus>();
        DataProtectorMock = new DataProtectorFixture()
            .SetupProtectToReturnInput()
            .SetupUnprotectToReturnInput()
            .Create();
        PublicPrivateKeyGenerator = new RsaPublicPrivateKeyGenerator(DataProtectorMock);
        BlockchainSeed = new BlockchainSeed(DomainBus, Mock.Of<IAggregateRootRepository<Blockchain>>());
        DateTimeProvider = new MockDateTimeProvider();
    }
}