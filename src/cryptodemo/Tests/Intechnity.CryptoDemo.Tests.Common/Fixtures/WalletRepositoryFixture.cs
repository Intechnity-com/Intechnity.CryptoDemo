using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Tests.Common.Fixtures;

public class WalletRepositoryFixture : BaseFixture<IAggregateRootRepository<Wallet>>
{
    public WalletRepositoryFixture SetupToReturnWallet(Wallet wallet)
    {
        MockedService.Setup(x => x.Get()).Returns(wallet);

        return this;
    }
}