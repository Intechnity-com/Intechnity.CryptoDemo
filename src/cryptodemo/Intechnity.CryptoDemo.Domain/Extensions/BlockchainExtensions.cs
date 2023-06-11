using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Domain.Extensions;

public static class BlockchainExtensions
{
    public static double GetSumOfDifficulties(this IEnumerable<Block> blocks)
    {
        return blocks.Select(x => Math.Pow(2, (double)x.MintingDifficulty))
            .DefaultIfEmpty(0)
            .Sum();
    }

    public static decimal? FindInputAmount(this IEnumerable<Block> blocks, TransactionInput transactionInput)
    {
        var block = blocks.FirstOrDefault(x => x.Index == transactionInput.PreviousBlockIndex);
        if (block == null)
            return null;

        var transaction = block.Transactions.FirstOrDefault(x => x.TransactionId == transactionInput.PreviousTransactionId);
        if (transaction == null)
            return null;

        var transactionOutput = transaction.TransactionOutputs.FirstOrDefault(x => x.Address == transactionInput.FromAddress);
        return transactionOutput?.Amount;
    }
}