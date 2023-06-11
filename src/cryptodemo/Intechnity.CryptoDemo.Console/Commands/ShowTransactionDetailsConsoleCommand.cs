using Spectre.Console;
using Intechnity.CryptoDemo.Console.Extensions;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowTransactionDetailsConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public ShowTransactionDetailsConsoleCommand(ITranslationProvider translationProvider,
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _translationProvider = translationProvider;
        _blockchainRepository = blockchainRepository;
    }

    public Task Execute()
    {
        var blockchain = _blockchainRepository.Get();
        var blockIndex = AnsiConsole.Ask<long>(_translationProvider.Translate("Enter block index:"));

        var block = blockchain.GetBlock(blockIndex);
        if (block == null)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("Block does not exist!"));
            return Task.CompletedTask;
        }

        var transactionId = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter transaction ID:"));
        var transaction = block.Transactions.FirstOrDefault(x => x.TransactionId == transactionId);
        if (transaction == null)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("Transaction does not exist!"));
            return Task.CompletedTask;
        }

        transaction.WriteToConsole(_translationProvider);

        return Task.CompletedTask;
    }
}