using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Extensions;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Domain.Tests.Domain.Transactions;

public class TransactionTests : BaseDomainTests
{
    [Fact]
    public void NewTransaction_TransactionIdIsProperlyCalculated()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        // hash was precalculated - it has to be always the same for this specific input
        transaction.TransactionId.Should().Be("abe9e1b3490fadd6f27f87f48b8aac98ac2649c24c4b78dd72ce03650bc5d12e");
    }

    [Fact]
    public void ValidateTransaction_EverythingAlright_ValidationSucceeds()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        var blockchain = blockchainBuilder.Build();

        transaction.ValidateTransaction(blockchain.Blocks, PublicPrivateKeyGenerator).Should().BeTrue();
    }

    [Fact]
    public void ValidateTransaction_InvalidTransactionId_ValidationFails()
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

        newTransaction.SetPrivatePropertyValue(nameof(newTransaction.TransactionId), newTransaction.TransactionId + "1");

        newTransaction.ValidateTransaction(blockchain.Blocks, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void ValidateTransaction_ContainsInvalidTransactionInputs_ValidationFails()
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

        foreach (var input in newTransaction.TransactionInputs)
        {
            input.SetPrivatePropertyValue(nameof(input.PreviousTransactionId), input.PreviousTransactionId + "1");
        }

        newTransaction.ValidateTransaction(blockchain.Blocks, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void ValidateTransaction_TotalsMismatch_ValidationFails()
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

        var output = newTransaction.TransactionOutputs[0];
        output.SetPrivatePropertyValue(nameof(output.Amount), output.Amount + 1);

        newTransaction.ValidateTransaction(blockchain.Blocks, PublicPrivateKeyGenerator).Should().BeFalse();
    }
}