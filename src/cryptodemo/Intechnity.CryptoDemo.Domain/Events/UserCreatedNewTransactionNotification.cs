using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Events;

public record UserCreatedNewTransactionNotification : IDomainNotification
{
    public List<ProtoTransaction> TransactionDTOs { get; init; }

    public UserCreatedNewTransactionNotification(List<ProtoTransaction> transactionDTOs)
    {
        TransactionDTOs = transactionDTOs;
    }
}