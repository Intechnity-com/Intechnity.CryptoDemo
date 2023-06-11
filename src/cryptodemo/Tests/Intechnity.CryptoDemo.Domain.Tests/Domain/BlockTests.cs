using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Helpers;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Extensions;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Domain.Tests.Domain;

public class BlockTests : BaseDomainTests
{
    [Fact]
    public void NewBlock_HashIsProperlyCalculated()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        // hash was precalculated - it has to be always the same for this specific input
        newBlock.Hash.Should().Be("ebd57570eab0b319f566f2659bf022ac2a199c75461ff3e12a162eddff83c279");
    }

    [Fact]
    public void Validate_EverythingAlright_ValidationSucceeds()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidIndex_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Index), newBlock.Index + 1);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidPreviousHash_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.PreviousHash), newBlock.PreviousHash + "1");

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();        
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidHash_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Hash), newBlock.Hash + "1");

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidTimestamp_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        var lastBlock = blockchain.Blocks.First();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Timestamp), lastBlock.Timestamp);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidBlockchainId_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.BlockchainId), newBlock.BlockchainId + "1");

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidVersion_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Version), newBlock.Version + "1");

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidDifficulty_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.MintingDifficulty), newBlock.MintingDifficulty - 1);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_NoTransactions_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .Build();

        var lastBlock = blockchain.Blocks.Last();

        var newBlock = new Block(
                CryptoDemoConsts.BLOCKCHAIN_ID,
                CryptoDemoConsts.BLOCKCHAIN_VERSION,
                lastBlock.Index + 1,
                WellKnownTestingData.SecondBlockTimestamp,
                new List<Transaction>(),
                CryptoDemoConsts.MINTING_DIFFICULTY,
                WellKnownTestingData.Address1PublicKey,
                lastBlock.Hash);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_ContainsInvalidTransactions_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchain = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .Build();

        var lastBlock = blockchain.Blocks.Last();

        var unspentTransactionOutputs = transactionPool.UnspentTransactionOutputs.Where(x => x.Address == WellKnownTestingData.Address1PublicKey).ToList();
        if (unspentTransactionOutputs == null)
            throw new InvalidOperationException();

        var newTransaction = TransactionsHelper.CreateUnsignedTransaction(WellKnownTestingData.Address1PublicKey, unspentTransactionOutputs, WellKnownTestingData.Address2PublicKey, unspentTransactionOutputs.Sum(x => x.Amount));

        // transactions are not signed so validation should fail

        var newBlock = new Block(
                CryptoDemoConsts.BLOCKCHAIN_ID,
                CryptoDemoConsts.BLOCKCHAIN_VERSION,
                lastBlock.Index + 1,
                WellKnownTestingData.SecondBlockTimestamp,
                new List<Transaction> { newTransaction },
                CryptoDemoConsts.MINTING_DIFFICULTY,
                WellKnownTestingData.Address1PublicKey,
                lastBlock.Hash);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void Validate_SameTransactionInputReferencedMutlipleTimes_ValidationFails()
    {
        // we do not support properly this logic yet - this unit test is to make sure that the check in the code is still there

        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction1 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3);

        var transaction2 = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address3PublicKey,
            4);
        blockchainBuilder.CreateBlockWithTransactions(new List<Transaction> { transaction1, transaction2 }, WellKnownTestingData.ThirdBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock);

        var blockchain = blockchainBuilder.Build();

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        newBlock.Validate(previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }
}