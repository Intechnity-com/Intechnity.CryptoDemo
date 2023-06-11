using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Extensions;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Domain.Tests.Domain;

public class BlockchainTests : BaseDomainTests
{
    [Fact]
    public async Task AppendMinedBlock_BlockAdded()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var builder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        builder.CreateBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3m,
            WellKnownTestingData.SecondBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var newBlock);

        var blockchain = builder.Build();
        var lastBlock = blockchain.Blocks.Last();

        DateTimeProvider.SetReturnDate(newBlock.Timestamp);
        await blockchain.ValidateAndAppendMyMinedBlock(newBlock, transactionPool, PublicPrivateKeyGenerator, DateTimeProvider);

        blockchain.Blocks.Last().Index.Should().Be(lastBlock.Index + 1);
    }

    [Fact]
    public void ValidateMinedBlock_InvalidBlockIndex_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var builder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        builder.CreateBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3m,
            WellKnownTestingData.SecondBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var newBlock);

        var blockchain = builder.Build();

        DateTimeProvider.SetReturnDate(newBlock.Timestamp);

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        blockchain.ValidateMinedBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator, DateTimeProvider).Should().BeTrue();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Index), newBlock.Index + 1);
        blockchain.ValidateMinedBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator, DateTimeProvider).Should().BeFalse();
    }

    [Fact]
    public void ValidateMinedBlock_InvalidTimestamp_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var builder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        builder.CreateBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3m,
            WellKnownTestingData.SecondBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var newBlock);

        var blockchain = builder.Build();

        DateTimeProvider.SetReturnDate(newBlock.Timestamp + CryptoDemoConsts.MAX_CLOCK_DIFF + TimeSpan.FromSeconds(1));

        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        blockchain.ValidateMinedBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator, DateTimeProvider).Should().BeFalse();
    }

    [Fact]
    public void ValidateBlock_InvalidPreviousBlockHash_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var builder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        builder.CreateBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3m,
            WellKnownTestingData.SecondBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var newBlock);

        var blockchain = builder.Build();

        DateTimeProvider.SetReturnDate(newBlock.Timestamp);
        
        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();        
        blockchain.ValidateBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeTrue();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Hash), newBlock.Hash + "1");
        blockchain.ValidateBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }

    [Fact]
    public void ValidateBlock_TimestampTooShort_ValidationFails()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var builder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        builder.CreateAndAppendBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            3m,
            WellKnownTestingData.SecondBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var previousBlock);

        builder.CreateBlockWithTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address3PublicKey,
            4m,
            WellKnownTestingData.ThirdBlockTimestamp,
            WellKnownTestingData.Address1PublicKey,
            out var newBlock);

        var blockchain = builder.Build();
        
        var previousBlocks = blockchain.Blocks.Where(x => x.Index < newBlock.Index).ToList();
        blockchain.ValidateBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeTrue();

        newBlock.SetPrivatePropertyValue(nameof(newBlock.Timestamp), previousBlock.Timestamp);
        blockchain.ValidateBlock(newBlock, previousBlocks, transactionPool, PublicPrivateKeyGenerator).Should().BeFalse();
    }
}