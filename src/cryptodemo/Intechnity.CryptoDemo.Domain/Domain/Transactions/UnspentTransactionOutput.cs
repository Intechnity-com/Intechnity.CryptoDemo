namespace Intechnity.CryptoDemo.Domain.Domain.Transactions;

public record UnspentTransactionOutput
{
    public string TransactionId { get; init; }

    public long BlockIndex { get; init; }

    public string Address { get; init; }

    public decimal Amount { get; init; }

    public UnspentTransactionOutput(string transactionId,
        long transactionIndex,
        string address,
        decimal amount)
    {
        TransactionId = transactionId;
        BlockIndex = transactionIndex;
        Address = address;
        Amount = amount;
    }
}