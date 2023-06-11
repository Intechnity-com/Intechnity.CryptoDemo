using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Services;
using Intechnity.CryptoDemo.Tests.Common.Fixtures;
using Intechnity.CryptoDemo.Tests.Common.Framework;

namespace Intechnity.CryptoDemo.Service.Tests.Services;

public class WalletPersistorTests : BaseDomainTests
{
    private const string WALLET_PASSWORD = "some long long password";

    [Fact]
    public async Task LoadWallet_ValidPassword_ProperlyLoadsWallet()
    {
        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);

        var walletRepositoryMock = new WalletRepositoryFixture()
            .SetupToReturnWallet(wallet)
            .Create();

        var walletPersistor = new WalletPersistor(DataProtectorMock, walletRepositoryMock);

        var walletPath = walletPersistor.GetWalletPath();
        File.WriteAllBytes(walletPath, Properties.Resources.test_wallet); // test wallet contains 2 accounts

        await walletPersistor.LoadWallet(WALLET_PASSWORD);

        wallet.Accounts.Should().HaveCount(2);
        wallet.Accounts.Should().ContainSingle(x => x.DisplayName == "test account 1");
        wallet.Accounts.Should().ContainSingle(x => x.DisplayName == "test account 2");
    }

    [Fact]
    public async Task LoadWallet_InvalidPassword_FailsToLoadWallet()
    {
        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);

        var walletRepositoryMock = new WalletRepositoryFixture()
            .SetupToReturnWallet(wallet)
            .Create();

        var walletPersistor = new WalletPersistor(DataProtectorMock, walletRepositoryMock);

        var walletPath = walletPersistor.GetWalletPath();
        File.WriteAllBytes(walletPath, Properties.Resources.test_wallet); // test wallet contains 2 accounts

        var action = () => walletPersistor.LoadWallet(WALLET_PASSWORD + "1111");
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SaveWallet_ProperlySavesWallet()
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();
        var account1 = new Account(address1, "test account 1");
        var account2 = new Account(address1, "test account 2");
        var account3 = new Account(address1, "test account 3");

        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1, account2, account3 });

        var walletRepositoryMock = new WalletRepositoryFixture()
            .SetupToReturnWallet(wallet)
            .Create();

        var walletPersistor = new WalletPersistor(DataProtectorMock, walletRepositoryMock);

        var walletPath = walletPersistor.GetWalletPath();
        if (File.Exists(walletPath))
            File.Delete(walletPath);

        await walletPersistor.SaveWallet(WALLET_PASSWORD);

        File.Exists(walletPath).Should().BeTrue();
    }
}