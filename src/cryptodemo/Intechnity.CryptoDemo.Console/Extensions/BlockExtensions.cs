using Spectre.Console;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Console.Extensions;

public static class BlockExtensions
{
    public static void WriteToConsole(this Block block, ITranslationProvider translationProvider)
    {
        var table = new Table();
        table.Title = new TableTitle(translationProvider.Translate("Block fields"));

        table.AddColumn(translationProvider.Translate("Field"));
        table.AddColumn(translationProvider.Translate("Value"));

        table.AddRow(nameof(block.BlockchainId), block.BlockchainId);
        table.AddRow(nameof(block.Version), block.Version);
        table.AddRow(nameof(block.Index), block.Index.ToString());
        table.AddRow(nameof(block.Timestamp), block.Timestamp.ToString());
        table.AddRow(nameof(block.MintingDifficulty), block.MintingDifficulty.ToString());
        table.AddRow(nameof(block.MinterAddress), block.MinterAddress);
        table.AddRow(nameof(block.PreviousHash), block.PreviousHash ?? "-");
        table.AddRow(nameof(block.Hash), block.Hash);

        AnsiConsole.Write(table);

        foreach (var transaction in block.Transactions)
        {
            AnsiConsole.WriteLine();
            transaction.WriteToConsole(translationProvider);
        }
    }
}