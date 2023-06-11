using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Console.Commands;

public class StopMintingConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAppState _appState;

    public StopMintingConsoleCommand(
        ITranslationProvider translationProvider,
        IAppState appState)
    {
        _translationProvider = translationProvider;
        _appState = appState;
    }

    public Task Execute()
    {
        if (!AnsiConsole.Confirm(_translationProvider.Translate("Are you sure you want to stop minting?")))
            return Task.CompletedTask;

        _appState.StopMinting();

        AnsiConsole.WriteLine(_translationProvider.Translate("Stopped minting"));

        return Task.CompletedTask;
    }
}