using Spectre.Console;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Console.Helpers;

public static class UserSelectHelper
{
    public static Account? AskUserToSelectAccount(ITranslationProvider translationProvider, IEnumerable<Account> accounts)
    {
        var accountChoiceMap = new Dictionary<string, Account>();
        int index = 1;
        foreach (var account in accounts)
        {
            var available = translationProvider.Translate("Available balance: {0}", account.AvailableBalance);
            var availableLocked = translationProvider.Translate("Available locked balance: {0}", account.AvailableLockedBalance);

            var key = $"{index}. {account.DisplayName} \t\t {available} \t\t {availableLocked}";
            accountChoiceMap.Add(key, account);
        }

        var accountInput = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(translationProvider.Translate("Please select an account"))
                        .PageSize(10)
                        .MoreChoicesText(translationProvider.Translate("[grey](Move up and down to reveal more accounts)[/]"))
                        .AddChoices(accountChoiceMap.Keys));

        if (!accountChoiceMap.ContainsKey(accountInput))
            return null;

        var result = accountChoiceMap[accountInput];
        return result;
    }
}