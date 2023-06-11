namespace Intechnity.CryptoDemo.Domain.Domain.Transactions;

public record TransactionOutput
{
    public string Address { get; init; }

    public decimal Amount { get; init; }

    public bool IsCoinbaseTransaction { get; init; }

    public TransactionOutput(string address, decimal amount, bool isCoinbaseTransaction)
    {
        Address = address;
        Amount = amount;
        IsCoinbaseTransaction = isCoinbaseTransaction;
    }
}