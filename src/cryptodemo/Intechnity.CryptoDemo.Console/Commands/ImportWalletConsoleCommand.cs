using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ImportWalletConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IWalletPersistor _walletPersistor;

    public ImportWalletConsoleCommand(
        ITranslationProvider translationProvider,
        IWalletPersistor walletPersistor)
    {
        _translationProvider = translationProvider;
        _walletPersistor = walletPersistor;
    }

    public async Task Execute()
    {
        var importPath = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter full path of wallet to import"));
        try
        {
            importPath = Path.GetFullPath(importPath);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("The provided path is not valid!"));
        }

        var password = UserWalletPasswordHelper.AskUserPasswordToDecryptWallet(_translationProvider);
        var loadedAccounts = await _walletPersistor.ImportWallet(importPath, password);

        if (loadedAccounts.Count == 0)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("No new accounts were loaded..."));
            return;
        }
        else
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("Loaded {0} new accounts", loadedAccounts.Count));

            password = UserWalletPasswordHelper.AskUserPasswordToEncryptWallet(_translationProvider);

            await _walletPersistor.SaveWallet(password);
        }
    }
}