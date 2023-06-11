using Spectre.Console;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Console.Extensions;

public static class TransactionExtensions
{
    public static void WriteToConsole(this Transaction transaction, ITranslationProvider translationProvider)
    {
        var table = new Table();
        table.Title = new TableTitle(translationProvider.Translate("Transaction {0}", transaction.TransactionId));
        table.AddColumn(translationProvider.Translate("Transactions"));

        var inputsTable = GetInputsTable(transaction.TransactionInputs, translationProvider);
        var outputsTable = GetOutputsTable(transaction.TransactionOutputs, translationProvider);

        table.AddRow(inputsTable);
        table.AddRow(outputsTable);

        AnsiConsole.Write(table);
    }

    private static Table GetOutputsTable(this IEnumerable<TransactionOutput> transactionOutputs, ITranslationProvider translationProvider)
    {
        var table = new Table();
        table.Title = new TableTitle(translationProvider.Translate("Outputs"));

        table.AddColumn(translationProvider.Translate("Address"));
        table.AddColumn(translationProvider.Translate("Amount"));
        table.AddColumn(translationProvider.Translate("Is Coinbase transaction"));

        foreach (var row in transactionOutputs)
        {
            table.AddRow(row.Address, row.Amount.ToString(), row.IsCoinbaseTransaction.ToString());
        }

        return table;
    }

    private static Table GetInputsTable(this IReadOnlyList<TransactionInput> transactionInputs, ITranslationProvider translationProvider)
    {
        var table = new Table();
        table.Title = new TableTitle(translationProvider.Translate("Inputs"));

        table.AddColumn(translationProvider.Translate("From address"));
        table.AddColumn(translationProvider.Translate("Previous Transaction Id"));
        table.AddColumn(translationProvider.Translate("Previous Block Index"));
        table.AddColumn(translationProvider.Translate("Signature"));

        foreach (var row in transactionInputs)
        {
            table.AddRow(row.FromAddress ?? "-", row.PreviousTransactionId ?? "-", row.PreviousBlockIndex.ToString(), row.Signature ?? "-");
        }

        return table;
    }
}