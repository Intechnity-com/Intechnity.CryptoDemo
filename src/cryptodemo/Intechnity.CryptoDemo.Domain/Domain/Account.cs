using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Domain.Domain;

public class Account
{
    private List<UnspentTransactionOutput> _unspentTransactionOutputs = new List<UnspentTransactionOutput>();
    private List<Transaction> _unconfirmedTransactions = new List<Transaction>();

    public PublicPrivateKeyPair PublicPrivateKeyPair { get; init; }

    public string DisplayName { get; private set; }

    public IReadOnlyList<UnspentTransactionOutput> AllUnspentTransactionOutputs => _unspentTransactionOutputs.AsReadOnly();

    public IReadOnlyList<Transaction> UnconfirmedTransactions => _unconfirmedTransactions.AsReadOnly();

    public IReadOnlyList<UnspentTransactionOutput> AvailableUnspentTransactionOutputs
    {
        get
        {
            var result = new List<UnspentTransactionOutput>();
            foreach (var utxo in _unspentTransactionOutputs)
            {
                if (UnconfirmedTransactions.Any(x => x.TransactionInputs.Any(b => b.PreviousTransactionId == utxo.TransactionId && b.FromAddress == utxo.Address)))
                    continue;

                result.Add(utxo);
            }

            return result;
        }
    }

    public decimal AvailableBalance => _unspentTransactionOutputs.Select(x => x.Amount).DefaultIfEmpty(0).Sum();

    public decimal AvailableLockedBalance => _unconfirmedTransactions
        .SelectMany(x => x.TransactionOutputs)
        .Where(x => x.Address == PublicPrivateKeyPair.PublicKey)
        .Select(x => x.Amount)
        .DefaultIfEmpty(0)
        .Sum();

    public decimal LockedBalance => _unconfirmedTransactions
        .SelectMany(x => x.TransactionOutputs)
        .Where(x => x.Address != PublicPrivateKeyPair.PublicKey)
        .Select(x => x.Amount)
        .DefaultIfEmpty(0)
        .Sum();

    public Account(PublicPrivateKeyPair publicPrivateKeyPair, string displayName, IEnumerable<UnspentTransactionOutput> unspentTransactionOutputs)
        : this(publicPrivateKeyPair, displayName)
    {
        _unspentTransactionOutputs.AddRange(unspentTransactionOutputs);
    }

    public Account(PublicPrivateKeyPair publicPrivateKeyPair, string displayName)
    {
        PublicPrivateKeyPair = publicPrivateKeyPair;
        DisplayName = displayName;
    }

    public void SetCurrentSate(List<UnspentTransactionOutput> unspentTransactionOutputs, List<Transaction> unconfirmedTransactions)
    {
        _unspentTransactionOutputs.Clear();
        _unconfirmedTransactions.Clear();

        _unspentTransactionOutputs.AddRange(unspentTransactionOutputs);

        foreach (var transaction in unconfirmedTransactions)
        {
            foreach (var txIn in transaction.TransactionInputs)
            {
                var unspentTransactionOutput = _unspentTransactionOutputs.FirstOrDefault(x => x.TransactionId == txIn.PreviousTransactionId);
                if (unspentTransactionOutput != null)
                {
                    // always the whole amount of an unspent transaction output is included in an transaction -> we can remove the uTXO from this account
                    _unspentTransactionOutputs.Remove(unspentTransactionOutput);
                }
            }

            _unconfirmedTransactions.Add(transaction);
        }
    }
}