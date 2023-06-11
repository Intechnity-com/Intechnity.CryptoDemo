using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Console.Commands;

public class StartMintingConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAppState _appState;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;

    public StartMintingConsoleCommand(
        ITranslationProvider translationProvider,
        IAppState appState,
        IAggregateRootRepository<Wallet> walletRepository)
    {
        _translationProvider = translationProvider;
        _appState = appState;
        _walletRepository = walletRepository;
    }

    public Task Execute()
    {
        var wallet = _walletRepository.Get();
        var accounts = wallet.Accounts;
        if (accounts.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No accounts in wallet available."));
            return Task.CompletedTask;
        }

        var account = UserSelectHelper.AskUserToSelectAccount(_translationProvider, accounts);
        if (account == null)
            return Task.CompletedTask;

        if (!AnsiConsole.Confirm(_translationProvider.Translate("Are you sure you want to start minting?")))
            return Task.CompletedTask;

        _appState.StartMinting(account);

        AnsiConsole.WriteLine(_translationProvider.Translate("Started minting using account {0}", account.DisplayName));

        return Task.CompletedTask;
    }
}