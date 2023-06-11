using Spectre.Console;
using Intechnity.CryptoDemo.Console.Commands;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Service.Services;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Console.App;

// For future release:
// Unit of work / transactions -> a database?
// Thread safe / synchronization context
// Check handling of decimal places - so we don't lose money due to inacuracy - 0.00001
// Update difficulty when after a certain threshold was reached
// Better handling of P2P communication - right now every node is connected to every node
// TransactionPool - caching could be removed or improved
// Better handling of blocks to download - only load when needed?
// Node types = full / partial
// user session - only enter password once at start
// add GUID to peers
// add IPC communication
// Split CryptoDemo into Blockchain / Data layer
// improve handling of transactions - right now user can create only 1 transaction per block
// Improve staking algorithm - properly handle staking of each user + introduce penalties
// Validate blockchain ID on connecting
// integration unit tests

public class AppHost : IHostedService
{
    private const string EXIT_COMMAND = "Exit";

    private readonly IHostApplicationLifetime _host;
    private readonly IServiceProvider _serviceProvider;

    public AppHost(IHostApplicationLifetime host,
        IServiceProvider serviceProvider)
    {
        _host = host;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            Task.Factory.StartNew(() => StartInternal(cancellationToken));

        return Task.CompletedTask;
    }

    private async void StartInternal(CancellationToken cancellationToken)
    {
        AnsiConsole.Write(@"

  _____       _            _           _ _         
 |_   _|     | |          | |         (_) |        
   | |  _ __ | |_ ___  ___| |__  _ __  _| |_ _   _ 
   | | | '_ \| __/ _ \/ __| '_ \| '_ \| | __| | | |
  _| |_| | | | ||  __/ (__| | | | | | | | |_| |_| |
 |_____|_| |_|\__\___|\___|_| |_|_| |_|_|\__|\__, |
                                              __/ |
                                             |___/ 
" +
        Environment.NewLine +
        Environment.NewLine +
        Environment.NewLine);

        await LoadApplication();

        var serviceScopeFactory = _serviceProvider!.GetRequiredService<IServiceScopeFactory>();
        var trans = _serviceProvider!.GetRequiredService<ITranslationProvider>();
        var appState = _serviceProvider!.GetRequiredService<IAppState>();

        var shouldExit = false;
        while (!shouldExit && !cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("-------------------------------------------------------------------");

            var availableCommands = GetCurrentlyAvailableCommands(trans, appState);
            var userInput = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select an operation")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more actions)[/]")
                    .AddChoices(availableCommands.Select(x => x.Name)));

            if (userInput == EXIT_COMMAND)
            {
                _host.StopApplication();
                shouldExit = true;
                break;
            }

            var command = availableCommands.FirstOrDefault(x => x.Name == userInput);
            if (command?.Command == null)
                continue;

            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var commandImplementation = ActivatorUtilities.CreateInstance(scope.ServiceProvider!, command.Command) as IConsoleCommand;
                    if (commandImplementation != null)
                    {
                        await commandImplementation.Execute();

                        AnsiConsole.Ask(trans.Translate("Press any key to continue..."), "continue");
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }

    private List<CommandInfo> GetCurrentlyAvailableCommands(ITranslationProvider trans, IAppState appState)
    {
        var availableCommands = new List<CommandInfo>
        {
            new CommandInfo { Name = trans.Translate("Show wallet informations"), Command = typeof(ShowWalletInfoConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Import account"), Command = typeof(ImportAccountConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Create new account"), Command = typeof(CreateNewAccountConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Export wallet"), Command = typeof(ExportWalletConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Import wallet"), Command = typeof(ImportWalletConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Send Transaction"), Command = typeof(SendTransactionConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Get balance of address"), Command = typeof(GetAddressBalanceConsoleCommand) },
            new CommandInfo { Name = trans.Translate("List transactions from-to address"), Command = typeof(ShowTransactionsFromToConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Show block details"), Command = typeof(ShowBlockDetailsConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Show transaction details"), Command = typeof(ShowTransactionDetailsConsoleCommand) },
            new CommandInfo { Name = trans.Translate("Show connected peers"), Command = typeof(ShowConnectedPeers) },
            new CommandInfo { Name = trans.Translate("Show unconfirmed transactions"), Command = typeof(ShowUnconfirmedTransactionsConsoleCommand) },
        };

        if (appState.IsMinting)
            availableCommands.Add(new CommandInfo { Name = trans.Translate("Stop minting"), Command = typeof(StopMintingConsoleCommand) });
        else
            availableCommands.Add(new CommandInfo { Name = trans.Translate("Start minting"), Command = typeof(StartMintingConsoleCommand) });

        availableCommands.Add(new CommandInfo { Name = EXIT_COMMAND });

        return availableCommands;
    }

    private async Task LoadApplication()
    {
        var trans = _serviceProvider.GetRequiredService<ITranslationProvider>();
        var serviceScopeFactory = _serviceProvider!.GetRequiredService<IServiceScopeFactory>();

        AnsiConsole.WriteLine(trans.Translate("Loading application..."));

        using (var scope = serviceScopeFactory.CreateScope())
        {
            AnsiConsole.WriteLine(trans.Translate("Seeding initial blockchain..."));
            var blockchainSeed = scope.ServiceProvider.GetRequiredService<IBlockchainSeed>();
            await blockchainSeed.SeedInitialBlockchainAsync(CryptoDemoConsts.GENESIS_BLOCK_MINTER_ADDRESS);

            var p2pMessagesBus = scope.ServiceProvider.GetRequiredService<IP2PManager>();

            var walletPersistor = scope.ServiceProvider.GetRequiredService<IWalletPersistor>();
            if (walletPersistor.CheckWalletExists())
            {
                try
                {
                    AnsiConsole.MarkupLine("[green]" + trans.Translate("Found a wallet to load...") + "[/]");
                    var password = UserWalletPasswordHelper.AskUserPasswordToDecryptWallet(trans);

                    AnsiConsole.WriteLine(trans.Translate("Loading wallet..."));
                    await walletPersistor.LoadWallet(password);

                    AnsiConsole.WriteLine(trans.Translate("Wallet was loaded:"));
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
            }
            else
            {
                AnsiConsole.WriteLine(trans.Translate("No wallet to load found."));
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Circle)
                .StartAsync(trans.Translate("Loading latest blocks and unconfirmed transactions from network..."), async ctx =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(CryptoDemoConsts.MAX_WAIT_TO_LOAD_BLOCKCHAIN);

                    while (!p2pMessagesBus.IsReady && !cancellationTokenSource.IsCancellationRequested)
                    {
                        await p2pMessagesBus.LoadInitialBlockchainAsync(cancellationTokenSource.Token);
                    }

                    if (p2pMessagesBus.IsReady)
                        AnsiConsole.MarkupLine("[green]" + trans.Translate("Successfully loaded blockchain from network") + "[/]");
                    else
                        AnsiConsole.MarkupLine("[red]" + trans.Translate("Failed to load blockchain from network") + "[/]");
                });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}