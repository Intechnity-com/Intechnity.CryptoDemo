using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Core.Extensions;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Protos;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Domain.Tests.Domain;

public class WalletTests : BaseDomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateNewTransaction_InvalidAmount_ThrowsException(decimal amount)
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();
        var address2 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();

        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(address1.PublicKey)
           .Build();

        var account1 = new Account(address1, "test account 1", transactionPool.GetUnspentTransactionOutputs(address1.PublicKey));

        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1 });

        var action = () => wallet.CreateNewTransaction(account1, address2.PublicKey, amount, PublicPrivateKeyGenerator);
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateNewTransaction_InsufficientBalande_ThrowsException()
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();
        var address2 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();

        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(address1.PublicKey)
           .Build();

        var account1 = new Account(address1, "test account 1", transactionPool.GetUnspentTransactionOutputs(address1.PublicKey));

        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1 });

        var action = () => wallet.CreateNewTransaction(account1, address2.PublicKey, account1.AvailableBalance + 1, PublicPrivateKeyGenerator);
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateNewTransaction_ToYourself_ThrowsException()
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();

        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(address1.PublicKey)
           .Build();

        var account1 = new Account(address1, "test account 1", transactionPool.GetUnspentTransactionOutputs(address1.PublicKey));

        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1 });

        var action = () => wallet.CreateNewTransaction(account1, address1.PublicKey, account1.AvailableBalance, PublicPrivateKeyGenerator);
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void CreateNewTransaction_ValidInput_LocksBalanceInAccount()
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();
        var address2 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();

        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(address1.PublicKey);

        var account1 = new Account(address1, "test account 1", transactionPool.GetUnspentTransactionOutputs(address1.PublicKey));
        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1 });

        var initialAvailableBalance = account1.AvailableBalance;
        var amountToSend = initialAvailableBalance - 10;
        var newTransaction = blockchainBuilder.CreateTransaction(
            address1.PublicKey,
            address1.ProtectedPrivateKey.BytesToHex(),
            address2.PublicKey,
            amountToSend);

        var blockchain = blockchainBuilder.Build();

        var dtos = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { newTransaction });
        var @event = new UserCreatedNewTransactionNotification(dtos);

        transactionPool.Apply(blockchain.Blocks, @event);

        wallet.UpdateWalletBalances(transactionPool);

        account1.UnconfirmedTransactions.Should().HaveCount(1);
        account1.AvailableBalance.Should().Be(0);
        account1.AvailableLockedBalance.Should().Be(initialAvailableBalance - amountToSend);
        account1.LockedBalance.Should().Be(amountToSend);
    }

    [Fact]
    public void AvailableUnspentTransactionOutputs_IsProperlyCalculated()
    {
        var address1 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();
        var address2 = PublicPrivateKeyGenerator.GeneratePublicPrivateKeyPair();

        var transactionPool = new TransactionPool(
            DomainBus,
            Mapper,
            DateTimeProvider,
            PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(
            DomainBus,
            Mapper,
            DateTimeProvider,
            BlockchainSeed,
            transactionPool, 
            PublicPrivateKeyGenerator)
           .SetGenesisBlock(address1.PublicKey)
           .CreateAndAppendBlockWithTransaction(
                address1.PublicKey,
                address1.ProtectedPrivateKey.BytesToHex(),
                address2.PublicKey,
                5m,
                WellKnownTestingData.SecondBlockTimestamp,
                address1.PublicKey,
                out var newBlock);

        var account1 = new Account(address1, "test account 1", transactionPool.GetUnspentTransactionOutputs(address1.PublicKey));
        var wallet = new Wallet(DomainBus, Mapper, DateTimeProvider);
        wallet.SetAccounts(new List<Account> { account1 });

        account1.AllUnspentTransactionOutputs.Should().HaveCount(2);
        account1.AvailableUnspentTransactionOutputs.Should().HaveCount(2);
        account1.UnconfirmedTransactions.Should().HaveCount(0);
        account1.AvailableBalance.Should().Be(2 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 5m);
        account1.AvailableLockedBalance.Should().Be(0);
        account1.LockedBalance.Should().Be(0);

        var initialAvailableBalance = account1.AvailableBalance;
        var amountToSend = initialAvailableBalance - 10;
        var newTransaction = blockchainBuilder.CreateTransaction(
            address1.PublicKey,
            address1.ProtectedPrivateKey.BytesToHex(),
            address2.PublicKey,
            amountToSend);

        var blockchain = blockchainBuilder.Build();

        var dtos = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { newTransaction });
        var @event = new UserCreatedNewTransactionNotification(dtos);

        transactionPool.Apply(blockchain.Blocks, @event);

        wallet.UpdateWalletBalances(transactionPool);

        account1.AllUnspentTransactionOutputs.Should().HaveCount(0);
        account1.AvailableUnspentTransactionOutputs.Should().HaveCount(0);
        account1.UnconfirmedTransactions.Should().HaveCount(1);
        account1.AvailableBalance.Should().Be(0);
        account1.AvailableLockedBalance.Should().Be(initialAvailableBalance - amountToSend);
        account1.LockedBalance.Should().Be(amountToSend);
    }
}