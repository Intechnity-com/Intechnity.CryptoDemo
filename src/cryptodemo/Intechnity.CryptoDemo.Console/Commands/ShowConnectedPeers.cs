using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowConnectedPeers : IConsoleCommand
{
    private readonly IP2PManager _p2pManager;
    private readonly ITranslationProvider _translationProvider;

    public ShowConnectedPeers(IP2PManager p2pManager, ITranslationProvider translationProvider)
    {
        _p2pManager = p2pManager;
        _translationProvider = translationProvider;
    }

    public Task Execute()
    {
        var peers = _p2pManager.KnownPeers.ToList();

        var table = new Table();
        table.AddColumn(_translationProvider.Translate("Peer"));

        foreach (var peer in peers)
        {
            table.AddRow(peer.EscapeMarkup());
        }

        AnsiConsole.Write(table);

        return Task.CompletedTask;
    }
}
