using FluentAssertions;
using System.Globalization;
using Xunit;
using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Protos;
using Intechnity.CryptoDemo.Tests.Common;
using Intechnity.CryptoDemo.Tests.Common.Framework;
using Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders;

namespace Intechnity.CryptoDemo.Service.Tests.Proto;

public class ProtoMappingTests : BaseDomainTests
{
    [Fact]
    public void TransactionMappings_DtoToProto_AreWorkingProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        var protoTransaction = Mapper.Map<ProtoTransaction>(transaction);

        protoTransaction.TransactionId.Should().Be(transaction.TransactionId);

        protoTransaction.TransactionInputs.Count.Should().Be(transaction.TransactionInputs.Count);
        for (int i = 0; i < transaction.TransactionInputs.Count; i++)
        {
            var protoInput = protoTransaction.TransactionInputs[i];
            var inputDTO = transaction.TransactionInputs[i];

            protoInput.FromAddress.Should().Be(inputDTO.FromAddress);
            protoInput.PreviousTransactionId.Should().Be(inputDTO.PreviousTransactionId);
            protoInput.PreviousBlockIndex.Should().Be(inputDTO.PreviousBlockIndex);
            protoInput.Signature.Should().Be(inputDTO.Signature);
        }

        protoTransaction.TransactionOutputs.Count.Should().Be(transaction.TransactionOutputs.Count);
        for (int i = 0; i < transaction.TransactionOutputs.Count; i++)
        {
            var protoOutput = protoTransaction.TransactionOutputs[i];
            var outputDTO = transaction.TransactionOutputs[i];

            protoOutput.Address.Should().Be(outputDTO.Address);
            protoOutput.Amount.Should().Be(outputDTO.Amount.ToString(CultureInfo.InvariantCulture));
            protoOutput.IsCoinbaseTransaction.Should().Be(outputDTO.IsCoinbaseTransaction);
        }
    }

    [Fact]
    public void TransactionMappings_ProtoToDto_AreWorkingProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        var blockchainBuilder = new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
           .SetGenesisBlock(WellKnownTestingData.Address1PublicKey);

        var transaction = blockchainBuilder.CreateTransaction(
            WellKnownTestingData.Address1PublicKey,
            WellKnownTestingData.Address1PrivateKey,
            WellKnownTestingData.Address2PublicKey,
            CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT / 2);

        var protoTransaction = Mapper.Map<ProtoTransaction>(transaction);

        // map back again :)
        transaction = Mapper.Map<Transaction>(protoTransaction);

        transaction.TransactionId.Should().Be(protoTransaction.TransactionId);

        transaction.TransactionInputs.Count.Should().Be(protoTransaction.TransactionInputs.Count);
        for (int i = 0; i < protoTransaction.TransactionInputs.Count; i++)
        {
            var protoInput = protoTransaction.TransactionInputs[i];
            var inputDTO = transaction.TransactionInputs[i];

            inputDTO.FromAddress.Should().Be(protoInput.FromAddress);
            inputDTO.PreviousTransactionId.Should().Be(protoInput.PreviousTransactionId);
            inputDTO.PreviousBlockIndex.Should().Be(protoInput.PreviousBlockIndex);
            inputDTO.Signature.Should().Be(protoInput.Signature);
        }

        transaction.TransactionOutputs.Count.Should().Be(protoTransaction.TransactionOutputs.Count);
        for (int i = 0; i < protoTransaction.TransactionOutputs.Count; i++)
        {
            var protoOutput = protoTransaction.TransactionOutputs[i];
            var outputDTO = transaction.TransactionOutputs[i];

            outputDTO.Address.Should().Be(protoOutput.Address);
            outputDTO.Amount.Should().Be(decimal.Parse(protoOutput.Amount, CultureInfo.InvariantCulture));
            outputDTO.IsCoinbaseTransaction.Should().Be(protoOutput.IsCoinbaseTransaction);
        }
    }

    [Fact]
    public void BlockMappings_DtoToProto_AreWorkingProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        var protoBlock = Mapper.Map<ProtoBlock>(newBlock);

        protoBlock.BlockchainId.Should().Be(newBlock.BlockchainId);
        protoBlock.Version.Should().Be(newBlock.Version);
        protoBlock.Index.Should().Be(newBlock.Index);
        protoBlock.Timestamp.ToDateTimeOffset().Should().Be(newBlock.Timestamp);
        protoBlock.Transactions.Should().HaveCount(newBlock.Transactions.Count);
        protoBlock.MintingDifficulty.Should().Be(newBlock.MintingDifficulty.ToString(CultureInfo.InvariantCulture));
        protoBlock.MinterAddress.Should().Be(newBlock.MinterAddress);
        protoBlock.PreviousHash.Should().Be(newBlock.PreviousHash);
        protoBlock.Hash.Should().Be(newBlock.Hash);
    }

    [Fact]
    public void BlockMappings_ProtoToDto_AreWorkingProperly()
    {
        var transactionPool = new TransactionPool(DomainBus, Mapper, DateTimeProvider, PublicPrivateKeyGenerator);
        new BlockchainTestDataBuilder(DomainBus, Mapper, DateTimeProvider, BlockchainSeed, transactionPool, PublicPrivateKeyGenerator)
            .SetGenesisBlock(WellKnownTestingData.Address1PublicKey)
            .CreateAndAppendBlockWithTransaction(WellKnownTestingData.Address1PublicKey, WellKnownTestingData.Address1PrivateKey, WellKnownTestingData.Address2PublicKey, 3m, WellKnownTestingData.SecondBlockTimestamp, WellKnownTestingData.Address1PublicKey, out var newBlock)
            .Build();

        var protoBlock = Mapper.Map<ProtoBlock>(newBlock);

        // now map back again
        newBlock = Mapper.Map<Block>(protoBlock);

        newBlock.BlockchainId.Should().Be(protoBlock.BlockchainId);
        newBlock.Version.Should().Be(protoBlock.Version);
        newBlock.Index.Should().Be(protoBlock.Index);
        newBlock.Timestamp.Should().Be(protoBlock.Timestamp.ToDateTimeOffset());
        newBlock.Transactions.Should().HaveCount(protoBlock.Transactions.Count);
        newBlock.MintingDifficulty.Should().Be(decimal.Parse(protoBlock.MintingDifficulty, CultureInfo.InvariantCulture));
        newBlock.MinterAddress.Should().Be(protoBlock.MinterAddress);
        newBlock.PreviousHash.Should().Be(protoBlock.PreviousHash);
        newBlock.Hash.Should().Be(protoBlock.Hash);
    }
}