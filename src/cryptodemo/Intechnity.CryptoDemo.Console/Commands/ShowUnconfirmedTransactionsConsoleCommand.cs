using Spectre.Console;
using Intechnity.CryptoDemo.Console.Extensions;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowUnconfirmedTransactionsConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;

    public ShowUnconfirmedTransactionsConsoleCommand(ITranslationProvider translationProvider,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository)
    {
        _translationProvider = translationProvider;
        _transactionPoolRepository = transactionPoolRepository;
    }

    public Task Execute()
    {
        var transactionPool = _transactionPoolRepository.Get();
        var transactions = transactionPool.UnconfirmedTransactions.Values.ToList();

        if (transactions.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No unconfirmed transactions found..."));
        }
        else
        {
            foreach (var transaction in transactions)
            {
                transaction.WriteToConsole(_translationProvider);
            }
        }

        return Task.CompletedTask;
    }
}