using System.Net.NetworkInformation;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Domain.Extensions;

public static class TransactionExtensions
{
    public static bool IsFromAddress(this Transaction transaction, string address)
    {
        return transaction.TransactionInputs.Any(x => x.FromAddress == address);
    }
}