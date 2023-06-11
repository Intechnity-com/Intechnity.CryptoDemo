using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Protos;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Domain.Tests.Domain;

public class TransactionPoolTests : BaseDomainTests
{
    [Fact]
    public void Apply_NewTransactionCreatedEvent_CreatesUnconfirmedTransaction()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var newTransaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        var blockchain = blockchainBuilder.Build();
        
        var dtos = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { newTransaction });
        var @event = new UserCreatedNewTransactionNotification(dtos);

        transactionPool.Apply(blockchain.Blocks, @event);

        transactionPool.UnconfirmedTransactions.Should().ContainKey(newTransaction.TransactionId);
    }

    [Fact]
    public void Apply_DuplicateTransactionEncountered_IsIgnored()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var newTransaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        var blockchain = blockchainBuilder.Build();

        var dtos = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { newTransaction });
        var @event = new UserCreatedNewTransactionNotification(dtos);

        transactionPool.Apply(blockchain.Blocks, @event);

        // and apply again
        transactionPool.Apply(blockchain.Blocks, @event);

        transactionPool.UnconfirmedTransactions.Should().ContainKey(newTransaction.TransactionId);
    }

    [Fact]
    public void ProcessAllTransactions_ProperlyCalculatesUnspentTransactionOutputs()
    {
        var arrangeTransactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, arrangeTransactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction1 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction1 }, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        var transaction2 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address3PublicKey,
            4);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction2 }, WellKnownTestingData.ThirdBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        var blockchain = blockchainBuilder.Build();

        var assertTransactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        assertTransactionPool.ProcessAllTransactions(blockchain.Blocks);

        var unconfirmedTransaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            1);
        var unconfirmedTransactionDTOs = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { transaction1, transaction2, unconfirmedTransaction });
        var @event = new UserCreatedNewTransactionNotification(unconfirmedTransactionDTOs);
        assertTransactionPool.Apply(blockchain.Blocks, @event);

        assertTransactionPool.ProcessAllTransactions(blockchain.Blocks);

        assertTransactionPool.UnconfirmedTransactions.Should().HaveCount(1);

        assertTransactionPool.UnspentTransactionOutputs.Should().ContainSingle(x => x.TransactionId == transaction1.TransactionId && x.Address == WellKnownTestingData.Address2PublicKey && x.Amount == 3);
        assertTransactionPool.UnspentTransactionOutputs.Should().ContainSingle(x => x.TransactionId == transaction2.TransactionId && x.Address == WellKnownTestingData.Address3PublicKey && x.Amount == 4);
        assertTransactionPool.UnspentTransactionOutputs.Where(x => x.Address == WellKnownTestingData.Address1PublicKey).Sum(x => x.Amount).Should().Be(3 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3 - 4);

        assertTransactionPool.BlockAddressBalanceInfos.Should().HaveCount(3);
        assertTransactionPool.BlockAddressBalanceInfos[0].AddressBalance[WellKnownTestingData.Address1PublicKey].Should().Be(CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);

        assertTransactionPool.BlockAddressBalanceInfos[1].AddressBalance[WellKnownTestingData.Address1PublicKey].Should().Be(2 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3);
        assertTransactionPool.BlockAddressBalanceInfos[1].AddressBalance[WellKnownTestingData.Address2PublicKey].Should().Be(3);

        assertTransactionPool.BlockAddressBalanceInfos[2].AddressBalance[WellKnownTestingData.Address1PublicKey].Should().Be(3 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3 - 4);
        assertTransactionPool.BlockAddressBalanceInfos[2].AddressBalance[WellKnownTestingData.Address2PublicKey].Should().Be(3);
        assertTransactionPool.BlockAddressBalanceInfos[2].AddressBalance[WellKnownTestingData.Address3PublicKey].Should().Be(4);

        assertTransactionPool.TransactionIdCache.Should().HaveCount(5);
    }

    [Fact]
    public void GetUnspentTransactionOutputs_WorksProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().HaveCount(1);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey)[0].Amount.Should().Be(CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);

        var transaction1 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction1 }, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().HaveCount(2);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().ContainSingle(x => x.Amount == CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().ContainSingle(x => x.Amount == CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address2PublicKey).Should().HaveCount(1);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address2PublicKey)[0].Amount.Should().Be(3);

        var transaction2 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address3PublicKey,
            4);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction2 }, WellKnownTestingData.ThirdBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().HaveCount(2);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().ContainSingle(x => x.Amount == CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address1PublicKey).Should().ContainSingle(x => x.Amount == 2 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3 - 4);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address2PublicKey).Should().HaveCount(1);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address2PublicKey)[0].Amount.Should().Be(3);

        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address3PublicKey).Should().HaveCount(1);
        transactionPool.GetUnspentTransactionOutputs(WellKnownTestingData.Address3PublicKey)[0].Amount.Should().Be(4);
    }

    [Fact]
    public void FindBalanceForAddress_WorksProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, null).Should().Be(CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);

        var transaction1 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction1 }, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, null).Should().Be(2 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address2PublicKey, null).Should().Be(3);

        var transaction2 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address3PublicKey,
            4);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction2 }, WellKnownTestingData.ThirdBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var _);

        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, null).Should().Be(3 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3 - 4);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address2PublicKey, null).Should().Be(3);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address3PublicKey, null).Should().Be(4);

        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, 1).Should().Be(1 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, 2).Should().Be(2 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address2PublicKey, 2).Should().Be(3);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address1PublicKey, 3).Should().Be(3 * CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT - 3 - 4);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address2PublicKey, 3).Should().Be(3);
        transactionPool.FindBalanceForAddress(WellKnownTestingData.Address3PublicKey, 3).Should().Be(4);
    }
}