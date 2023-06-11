using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record NewTransactionDistributedNotification : IDomainNotification
{
    public List<ProtoTransaction> TransactionDTOs { get; init; }

    public NewTransactionDistributedNotification(List<ProtoTransaction> transactionDTOs)
    {
        TransactionDTOs = transactionDTOs;
    }
}