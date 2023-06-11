using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Domain.Helpers;

public static class TransactionsHelper
{
    public static Transaction CreateUnsignedTransaction(string fromAddress, IReadOnlyList<UnspentTransactionOutput> unspentTransaction, string toAddress, decimal amountToSend)
    {
        var allUnspentAmount = unspentTransaction.Sum(x => x.Amount);

        if (amountToSend > allUnspentAmount)
            throw new InvalidOperationException("Not enough money to send this transaction");

        if (unspentTransaction.Any(x => x.Address != fromAddress))
            throw new InvalidOperationException("All unspent transaction must be from the same address");

        var leftoverAmount = allUnspentAmount - amountToSend;
        var transactionInputs = unspentTransaction.Select(x => new TransactionInput(x.Address, x.TransactionId, x.BlockIndex)).ToList();

        var transactionOutputs = new List<TransactionOutput>
        {
            new TransactionOutput(toAddress, amountToSend, false)
        };

        if (leftoverAmount > 0)
            transactionOutputs.Add(new TransactionOutput(fromAddress, leftoverAmount, false));

        return new Transaction(transactionInputs, transactionOutputs);
    }

    public static Transaction CreateCoinbaseTransaction(string minterAddress)
    {
        var transactionInput = new TransactionInput(null, null, -1);
        var transactionOutput = new TransactionOutput(minterAddress, CryptoDemoConsts.COINBASE_TRANSACTION_AMOUNT, true);

        var transaction = new Transaction(
            new List<TransactionInput> { transactionInput },
            new List<TransactionOutput> { transactionOutput });

        return transaction;
    }
}