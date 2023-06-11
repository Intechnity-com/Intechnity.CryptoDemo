using AutoMapper;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Extensions;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Domain;

public class TransactionPool : AggregateRoot
{
    private readonly object _lock = new object();

    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

    private Dictionary<string, Transaction> _unconfirmedTransactions = new Dictionary<string, Transaction>();
    private List<UnspentTransactionOutput> _unspentTransactionOutputs = new List<UnspentTransactionOutput>();
    private Dictionary<long, BlockAddressBalanceInfo> _blockAddressBalanceInfos = new Dictionary<long, BlockAddressBalanceInfo>();
    private Dictionary<string, Transaction> _transactionCache = new Dictionary<string, Transaction>();

    public TransactionPool(
        IDomainBus bus,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
        : base(bus, mapper, dateTimeProvider)
    {
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
    }

    public IReadOnlyDictionary<string, Transaction> UnconfirmedTransactions => _unconfirmedTransactions;

    public IReadOnlyCollection<UnspentTransactionOutput> UnspentTransactionOutputs => _unspentTransactionOutputs.AsReadOnly();

    public IReadOnlyDictionary<long, BlockAddressBalanceInfo> BlockAddressBalanceInfos => _blockAddressBalanceInfos;

    public IReadOnlyDictionary<string, Transaction> TransactionIdCache => _transactionCache;

    public void Apply(IEnumerable<Block> blocks, UserCreatedNewTransactionNotification @event)
    {
        AddUnconfirmedTransactions(blocks, @event.TransactionDTOs);
    }

    public void Apply(IEnumerable<Block> blocks, NewTransactionDistributedNotification @event)
    {
        AddUnconfirmedTransactions(blocks, @event.TransactionDTOs);
    }

    public void AddUnconfirmedTransactions(IEnumerable<Block> blocks, List<ProtoTransaction> transactionDTOs)
    {
        var transactions = Mapper.Map<List<Transaction>>(transactionDTOs);
        foreach (var transaction in transactions)
        {
            if (!transaction.ValidateTransaction(blocks, _publicPrivateKeyGenerator))
                continue;

            if (_unconfirmedTransactions.ContainsKey(transaction.TransactionId))
                continue;

            _unconfirmedTransactions.Add(transaction.TransactionId, transaction);
        }
    }

    public decimal FindBalanceForAddress(string address, long? atBlockIndex)
    {
        var entriesToCheck = atBlockIndex.HasValue ?
            _blockAddressBalanceInfos.Where(x => x.Key < atBlockIndex) : // the prior block has to be taken!
            _blockAddressBalanceInfos;
        var lastEntry = entriesToCheck.LastOrDefault(x => x.Value.HasBalanceForAddress(address));

        if (!lastEntry.Equals(default(KeyValuePair<long, BlockAddressBalanceInfo>)))
            return lastEntry.Value.GetAddressBalance(address);

        return 0;
    }

    public List<UnspentTransactionOutput> GetUnspentTransactionOutputs(string address)
    {
        var unspentTransactionOutputs = _unspentTransactionOutputs.Where(x => x.Address == address).ToList();
        return unspentTransactionOutputs;
    }

    public List<Transaction> GetUnconfirmedTransactions(string address)
    {
        return _unconfirmedTransactions.Where(x => x.Value.IsFromAddress(address)).Select(x => x.Value).ToList();
    }

    public void ProcessAllTransactions(IReadOnlyCollection<Block> blocks)
    {
        lock (_lock)
        {
            _unspentTransactionOutputs.Clear();
            _blockAddressBalanceInfos.Clear();
            _transactionCache.Clear();

            foreach (var block in blocks)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (_unconfirmedTransactions.ContainsKey(transaction.TransactionId))
                        _unconfirmedTransactions.Remove(transaction.TransactionId);

                    foreach (var input in transaction.TransactionInputs)
                    {
                        var unspentOutputToRemove = _unspentTransactionOutputs.SingleOrDefault(x => x.BlockIndex == input.PreviousBlockIndex && x.TransactionId == input.PreviousTransactionId && x.Address == input.FromAddress);
                        if (unspentOutputToRemove != null)
                            _unspentTransactionOutputs.Remove(unspentOutputToRemove);
                    }

                    foreach (var output in transaction.TransactionOutputs)
                    {
                        var newUnspentTransaction = new UnspentTransactionOutput(transaction.TransactionId, block.Index, output.Address, output.Amount);
                        _unspentTransactionOutputs.Add(newUnspentTransaction);
                    }

                    var transactionKey = GetKeyForTransaction(block.Index, transaction.TransactionId);
                    _transactionCache[transactionKey] = transaction;
                }

                var blockAddressBalanceInfos = new BlockAddressBalanceInfo();
                foreach (var addressGroup in _unspentTransactionOutputs.GroupBy(x => x.Address))
                {
                    var address = addressGroup.Key;
                    var balance = addressGroup.Sum(x => x.Amount);

                    blockAddressBalanceInfos.SetAddressBalance(address, balance);
                }

                _blockAddressBalanceInfos[block.Index] = blockAddressBalanceInfos;
            }

            // this is slow... don't recalculate known and valid blocks - but for first POC it will be enough
        }
    }

    private string GetKeyForTransaction(long blockIndex, string transactionId)
    {
        return blockIndex + "|" + transactionId;
    }
}