using MediatR;
using Spectre.Console;
using Intechnity.CryptoDemo.Console.Framework;
using Intechnity.CryptoDemo.Console.Helpers;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Console.Commands;

public class ImportAccountConsoleCommand : IConsoleCommand
{
    private readonly IDomainBus _domainBus;
    private readonly ITranslationProvider _translationProvider;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;
    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;
    private readonly IWalletPersistor _walletPersistor;

    public ImportAccountConsoleCommand(IDomainBus domainBus,
        ITranslationProvider translationProvider,
        IAggregateRootRepository<Wallet> walletRepository,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator,
        IWalletPersistor walletPersistor)
    {
        _domainBus = domainBus;
        _translationProvider = translationProvider;
        _walletRepository = walletRepository;
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
        _walletPersistor = walletPersistor;
    }

    public async Task Execute()
    {
        var wallet = _walletRepository.Get();

        var displayName = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter a name for the account: "));
        var publicKey = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter the public key: "));
        var privateKey = AnsiConsole.Ask<string>(_translationProvider.Translate("Enter the private key: "));
        var newPublicPrivateKeyPair = _publicPrivateKeyGenerator.GetPublicPrivateKeyPair(publicKey, privateKey); ;

        var newAccount = new Account(newPublicPrivateKeyPair, displayName);
        wallet.AddNewAccount(newAccount);

        AnsiConsole.WriteLine(_translationProvider.Translate("Created new account '{0}' - {1}", newAccount.DisplayName, newAccount.PublicPrivateKeyPair.PublicKey));

        var password = UserWalletPasswordHelper.AskUserPasswordToEncryptWallet(_translationProvider);
        await _walletPersistor.SaveWallet(password);

        await _domainBus.RaiseImmediately(new LocalBlockchainStateChangedNotification());
    }
}
