using MediatR;
using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class GetAddressBalanceConsoleCommand : IConsoleCommand
{
    private readonly IMediator _mediator;
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;

    public GetAddressBalanceConsoleCommand(IMediator mediator,
        ITranslationProvider translationProvider,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository)
    {
        _mediator = mediator;
        _translationProvider = translationProvider;
        _transactionPoolRepository = transactionPoolRepository;
    }

    public Task Execute()
    {
        var transactionPool = _transactionPoolRepository.Get();

        var address = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter the address:"));
        var balance = transactionPool.FindBalanceForAddress(address, null);

        AnsiConsole.WriteLine("Balance: {0}", balance);

        return Task.CompletedTask;
    }
}