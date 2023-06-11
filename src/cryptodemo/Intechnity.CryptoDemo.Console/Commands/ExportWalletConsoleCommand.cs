using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ExportWalletConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;
    private readonly IWalletPersistor _walletPersistor;

    public ExportWalletConsoleCommand(
        ITranslationProvider translationProvider,
        IAggregateRootRepository<Wallet> walletRepository,
        IWalletPersistor walletPersistor)
    {
        _translationProvider = translationProvider;
        _walletRepository = walletRepository;
        _walletPersistor = walletPersistor;
    }

    public async Task Execute()
    {
        var wallet = _walletRepository.Get();
        if (wallet.Accounts.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No account created yet!"));
        }

        var exportPath = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter full export path"));
        try
        {
            exportPath = Path.GetFullPath(exportPath);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("The provided path is not valid!"));
        }

        var password = UserWalletPasswordHelper.AskUserPasswordToDecryptWallet(_translationProvider);
        if (!AnsiConsole.Confirm(_translationProvider.Translate("Are you sure you want to export your wallet to {0}", exportPath)))
            return;

        await _walletPersistor.ExportWallet(exportPath, password);

        AnsiConsole.WriteLine(_translationProvider.Translate("Your wallet was exported to {0}", exportPath));
    }
}