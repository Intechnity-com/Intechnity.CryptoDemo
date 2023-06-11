using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Helpers;

namespace Intechnity.CryptoDemo.Domain.Tests.Helpers;

public class TransactionsHelperTests
{
    [Fact]
    public void CreateUnsignedTransaction_SingleUnspentTransactionOutput_AmountLowerThanAvailable_AdditionalOutputTransactionIsCreated()
    {
        decimal availableAmount = 10;
        var myAddress = "myAddress";
        var unspentTransactionOutput = new List<UnspentTransactionOutput>
        {
            new UnspentTransactionOutput("1", 1, myAddress, availableAmount)
        };

        decimal moneyToSend = 6;
        var toAddress = "toAddress";
        var resultTransaction = TransactionsHelper.CreateUnsignedTransaction(myAddress, unspentTransactionOutput, toAddress, moneyToSend);

        resultTransaction.TransactionInputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Count.Should().Be(2);
        resultTransaction.TransactionOutputs[0].Amount.Should().Be(moneyToSend);
        resultTransaction.TransactionOutputs[0].Address.Should().Be(toAddress);
        resultTransaction.TransactionOutputs[1].Amount.Should().Be(availableAmount - moneyToSend);
        resultTransaction.TransactionOutputs[1].Address.Should().Be(myAddress);
    }

    [Fact]
    public void CreateUnsignedTransaction_SingleUnspentTransactionOutput_AmountEqualToAvailable_NoAdditionalOutputTransactionIsCreated()
    {
        var amount = 10m;
        var myAddress = "myAddress";
        var unspentTransactionOutput = new List<UnspentTransactionOutput>
        {
            new UnspentTransactionOutput("1", 1, myAddress, amount)
        };

        var toAddress = "toAddress";
        var resultTransaction = TransactionsHelper.CreateUnsignedTransaction(myAddress, unspentTransactionOutput, toAddress, amount);

        resultTransaction.TransactionInputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Count.Should().Be(1);
        resultTransaction.TransactionOutputs[0].Amount.Should().Be(amount);
        resultTransaction.TransactionOutputs[0].Address.Should().Be(toAddress);
    }

    [Fact]
    public void CreateUnsignedTransaction_MultipleUnspentTransactionOutput_AmountLowerThanAvailable_AdditionalOutputTransactionIsCreated()
    {
        var myAddress = "myAddress";
        var unspentTransactionOutput = new List<UnspentTransactionOutput>
        {
            new UnspentTransactionOutput("1", 1, myAddress, 20m),
            new UnspentTransactionOutput("2", 2, myAddress, 30m)
        };

        decimal moneyToSend = 6;
        var toAddress = "toAddress";
        var resultTransaction = TransactionsHelper.CreateUnsignedTransaction(myAddress, unspentTransactionOutput, toAddress, moneyToSend);

        resultTransaction.TransactionInputs.Should().HaveCount(2);
        resultTransaction.TransactionOutputs.Should().HaveCount(2);
        resultTransaction.TransactionOutputs[0].Amount.Should().Be(moneyToSend);
        resultTransaction.TransactionOutputs[0].Address.Should().Be(toAddress);
        resultTransaction.TransactionOutputs[1].Amount.Should().Be(50m - moneyToSend);
        resultTransaction.TransactionOutputs[1].Address.Should().Be(myAddress);
    }

    [Fact]
    public void CreateUnsignedTransaction_MultipleUnspentTransactionOutput_AmountEqualToAvailable_NoAdditionalOutputTransactionIsCreated()
    {
        var myAddress = "myAddress";
        var unspentTransactionOutput = new List<UnspentTransactionOutput>
        {
            new UnspentTransactionOutput("1", 1, myAddress, 20m),
            new UnspentTransactionOutput("2", 2, myAddress, 30m)
        };

        var toAddress = "toAddress";
        var resultTransaction = TransactionsHelper.CreateUnsignedTransaction(myAddress, unspentTransactionOutput, toAddress, 50m);

        resultTransaction.TransactionInputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Should().NotBeNull();
        resultTransaction.TransactionOutputs.Count.Should().Be(1);
        resultTransaction.TransactionOutputs[0].Amount.Should().Be(50m);
        resultTransaction.TransactionOutputs[0].Address.Should().Be(toAddress);
    }
}