using AutoMapper;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Helpers;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Domain;

public class Wallet : AggregateRoot, IApply<UserCreatedNewTransactionNotification>
{
    private List<Account> _accounts = new List<Account>();

    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    public Wallet(IDomainBus bus, IMapper mapper, IDateTimeProvider dateTimeProvider)
        : base(bus, mapper, dateTimeProvider)
    {
    }

    public void SetAccounts(IEnumerable<Account> accounts)
    {
        _accounts.AddRange(accounts);
    }

    public void AddNewAccount(Account newAccount)
    {
        _accounts.Add(newAccount);
    }

    public void UpdateWalletBalances(TransactionPool transactionPool)
    {
        foreach (var account in _accounts)
        {
            var unspentTransactionOutput = transactionPool.GetUnspentTransactionOutputs(account.PublicPrivateKeyPair.PublicKey);
            var unconfirmedTransactions = transactionPool.GetUnconfirmedTransactions(account.PublicPrivateKeyPair.PublicKey);
            account.SetCurrentSate(unspentTransactionOutput, unconfirmedTransactions);
        }
    }

    public async Task CreateNewTransaction(Account fromAccount, string toAddress, decimal amount, IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Wrong amount");

        if (fromAccount.AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient balance");

        if (toAddress == fromAccount.PublicPrivateKeyPair.PublicKey)
            throw new InvalidOperationException("Can't transfer money to yourself!");

        var unspentTransactions = fromAccount.AvailableUnspentTransactionOutputs;
        var newTransaction = TransactionsHelper.CreateUnsignedTransaction(fromAccount.PublicPrivateKeyPair.PublicKey, unspentTransactions, toAddress, amount);
        foreach (var txIn in newTransaction.TransactionInputs)
            txIn.Sign(fromAccount.PublicPrivateKeyPair.ProtectedPrivateKey, publicPrivateKeyGenerator);

        var dtos = Mapper.Map<List<ProtoTransaction>>(new List<Transaction> { newTransaction });
        var @event = new UserCreatedNewTransactionNotification(dtos);
        await RaiseEvent(@event);
    }

    public void Apply(UserCreatedNewTransactionNotification @event)
    {
        // nothing to do here...
    }
}