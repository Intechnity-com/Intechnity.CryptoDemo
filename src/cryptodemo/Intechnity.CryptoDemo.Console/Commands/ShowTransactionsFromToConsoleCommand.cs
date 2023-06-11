using MediatR;
using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Extensions;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowTransactionsFromToConsoleCommand : IConsoleCommand
{
    private readonly IMediator _mediator;
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public ShowTransactionsFromToConsoleCommand(IMediator mediator,
        ITranslationProvider translationProvider,
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _mediator = mediator;
        _translationProvider = translationProvider;
        _blockchainRepository = blockchainRepository;
    }

    public Task Execute()
    {
        var blockchain = _blockchainRepository.Get();

        var fromAddress = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter sender"));
        var toAddress = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter receiver"));

        var foundAnything = false;
        foreach (var block in blockchain.Blocks)
        {
            // this is very slow - but sufficient for first release
            foreach (var transaction in block.Transactions.Where(x => x.IsFromAddress(fromAddress)))
            {
                foreach (var output in transaction.TransactionOutputs)
                {
                    if (output.Address == toAddress)
                    {
                        AnsiConsole.WriteLine(_translationProvider.Translate("Transaction ID: {0}, Amount: {1}", transaction.TransactionId, output.Amount));
                        foundAnything = true;
                    }
                }
            }
        }

        if (!foundAnything)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No transactions found..."));
        }

        return Task.CompletedTask;
    }
}