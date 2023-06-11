using Spectre.Console;
using Intechnity.CryptoDemo.Console.Extensions;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ShowBlockDetailsConsoleCommand : IConsoleCommand
{
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public ShowBlockDetailsConsoleCommand(ITranslationProvider translationProvider,
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _translationProvider = translationProvider;
        _blockchainRepository = blockchainRepository;
    }

    public Task Execute()
    {
        var blockchain = _blockchainRepository.Get();
        var blockIndex = AnsiConsole.Ask<long>(_translationProvider.Translate("Enter block index:"));
        var block = blockchain.GetBlock(blockIndex);

        if (block == null)
        {
            AnsiConsole.WriteLine(_translationProvider.Translate("Block does not exist!"));
            return Task.CompletedTask;
        }

        block.WriteToConsole(_translationProvider);

        return Task.CompletedTask;
    }
}