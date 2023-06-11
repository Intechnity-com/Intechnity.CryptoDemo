using MediatR;
using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class SendTransactionConsoleCommand : IConsoleCommand
{
    private readonly IMediator _mediator;
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;
    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

    public SendTransactionConsoleCommand(IMediator mediator,
        ITranslationProvider translationProvider,
        IAggregateRootRepository<Wallet> walletRepository,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        _mediator = mediator;
        _translationProvider = translationProvider;
        _walletRepository = walletRepository;
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
    }

    public async Task Execute()
    {
        var wallet = _walletRepository.Get();
        var accounts = wallet.Accounts;
        if (accounts.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No accounts in wallet available."));
            return;
        }

        var fromAccount = UserSelectHelper.AskUserToSelectAccount(_translationProvider, accounts);
        if (fromAccount == null)
            return;

        var toAddress = AnsiConsole.Ask<string>(_translationProvider.Translate("To what address do you want to send the transaction?"));
        var amount = AnsiConsole.Ask<decimal>(_translationProvider.Translate("How many coins do you want to send?"));

        if (!AnsiConsole.Confirm(_translationProvider.Translate("Are you sure you want to send out the transaction?")))
            return;

        AnsiConsole.MarkupLine(_translationProvider.Translate("Creating new transaction..."));

        await wallet.CreateNewTransaction(fromAccount, toAddress, amount, _publicPrivateKeyGenerator); // maybe better send command to mediatR?
    }
}