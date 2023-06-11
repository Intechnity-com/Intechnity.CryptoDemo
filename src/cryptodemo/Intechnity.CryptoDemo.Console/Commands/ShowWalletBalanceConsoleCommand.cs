using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowWalletInfoConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;

    public ShowWalletInfoConsoleCommand(ITranslationProvider translationProvider,
        IAggregateRootRepository<Wallet> walletRepository)
    {
        _translationProvider = translationProvider;
        _walletRepository = walletRepository;
    }

    public Task Execute()
    {
        var wallet = _walletRepository.Get();
        if (wallet.Accounts.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No accounts available!"));
            return Task.CompletedTask;
        }

        foreach (var account in wallet.Accounts)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("{0}: {1}", account.DisplayName, account.PublicPrivateKeyPair.PublicKey));
        }

        var table = new Table();
        table.AddColumn(_translationProvider.Translate("Account"));
        table.AddColumn(_translationProvider.Translate("Available balance"));
        table.AddColumn(_translationProvider.Translate("Available locked balance"));
        table.AddColumn(_translationProvider.Translate("Pending outgoing balance"));
        table.AddColumn(_translationProvider.Translate("Address"));

        foreach (var account in wallet.Accounts)
        {
            table.AddRow(
                account.DisplayName,
                account.AvailableBalance.ToString(),
                account.AvailableLockedBalance.ToString(),
                account.LockedBalance.ToString(),
                account.PublicPrivateKeyPair.PublicKey);
        }

        AnsiConsole.Write(table);

        return Task.CompletedTask;
    }
}