using Spectre.Console;
using Intechnity.CryptoDemo.Core.Providers;

namespace Intechnity.CryptoDemo.Console.Helpers;

// DT - I don't like this approach, this mechanism should be improved.
public static class UserWalletPasswordHelper
{
    public static string AskUserPasswordToDecryptWallet(ITranslationProvider translationProvider)
    {
        var prompt = new TextPrompt<string>(translationProvider.Translate("Please enter [green]password[/] which was used to encrypt the wallet"))
            .Secret();
        return AnsiConsole.Prompt(prompt);
    }

    public static string AskUserPasswordToEncryptWallet(ITranslationProvider translationProvider)
    {
        AnsiConsole.WriteLine(translationProvider.Translate("Your wallet has changed - please enter password that will be used to persist your wallet."));
        
        AnsiConsole.WriteLine(translationProvider.Translate("The password will be used to encrypt your whole wallet - including previous accounts!"));
        var prompt = new TextPrompt<string>(translationProvider.Translate("Enter [green]password[/]"))
            .Secret();
        return AnsiConsole.Prompt(prompt);
    }
}
