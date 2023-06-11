using System.Globalization;
using System.Text;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Tools;
using Intechnity.CryptoDemo.Domain.Extensions;

namespace Intechnity.CryptoDemo.Domain.Domain.Transactions;

public record Transaction
{
    private List<TransactionInput> _transactionInputs = new List<TransactionInput>();
    private List<TransactionOutput> _transactionOutputs = new List<TransactionOutput>();

    public IReadOnlyList<TransactionInput> TransactionInputs => _transactionInputs.AsReadOnly();

    public IReadOnlyList<TransactionOutput> TransactionOutputs => _transactionOutputs.AsReadOnly();

    public string TransactionId { get; init; }

    public Transaction()
    {
    }

    public Transaction(IEnumerable<TransactionInput> transactionInputs, IEnumerable<TransactionOutput> transactionsOutputs)
    {
        _transactionInputs.AddRange(transactionInputs);
        _transactionOutputs.AddRange(transactionsOutputs);
        TransactionId = CalculateTransactionId();
    }

    public bool ValidateTransaction(IEnumerable<Block> blocks, IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        if (TransactionInputs.Count == 0)
            return false;

        if (TransactionOutputs.Count < 1) // always 1 coinbase
            return false;

        var calcualtedTransactionId = CalculateTransactionId();
        if (calcualtedTransactionId != TransactionId)
            return false;

        if (TransactionInputs.Any(x => !x.Validate(publicPrivateKeyGenerator)))
            return false;

        if (TransactionOutputs.Count(x => x.IsCoinbaseTransaction) > 1)
            return false;

        if (TransactionInputs.GroupBy(x => x.PreviousTransactionId).Any(x => x.Count() > 1))
            return false; // duplicated inputs

        if (TransactionInputs.Select(x => x.FromAddress).Distinct().Count() > 1)
            return false; // must come from 1 address

        if (TransactionOutputs.GroupBy(x => x.Address).Any(x => x.Count() > 1))
            return false; // max 1 output per address - logic is dependent on this!

        var totalOutput = TransactionOutputs.Where(x => !x.IsCoinbaseTransaction).Sum(x => x.Amount);
        decimal totalInput = 0;
        foreach (var input in TransactionInputs)
        {
            if (input.PreviousBlockIndex == -1)
                continue;

            var inputAmount = blocks.FindInputAmount(input);
            if (inputAmount == null)
                return false;

            totalInput += inputAmount.Value;
        }

        if (totalOutput != totalInput)
            return false;

        return true;
    }

    private string CalculateTransactionId()
    {
        var sb = new StringBuilder();

        foreach (var txIn in TransactionInputs.OrderBy(x => x.PreviousBlockIndex))
        {
            sb.Append(txIn.PreviousTransactionId);
            sb.Append(txIn.PreviousBlockIndex);
        }

        foreach (var txOut in TransactionOutputs.OrderBy(x => x.Address).ThenBy(x => x.Amount))
        {
            sb.Append(txOut.Address);
            sb.Append(txOut.Amount.ToString(CultureInfo.InvariantCulture));
        }

        var id = HashTools.Sha256Hash(sb.ToString());

        return id;
    }
}