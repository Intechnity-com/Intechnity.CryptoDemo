using Newtonsoft.Json;
using System.Text;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Tools;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.Services;

public class WalletPersistor : IWalletPersistor
{
    private const string DEFAULT_WALLET_PATH = ".zcw";

    private readonly IDataProtector _dataProtector;
    private readonly IAggregateRootRepository<Wallet> _walletRepository;

    public WalletPersistor(IDataProtector dataProtector, IAggregateRootRepository<Wallet> walletRepository)
    {
        _dataProtector = dataProtector;
        _walletRepository = walletRepository;
    }

    public bool CheckWalletExists()
    {
        return File.Exists(GetWalletPath());
    }

    public async Task LoadWallet(string password)
    {
        if (!CheckWalletExists())
            throw new InvalidOperationException("wallet does not exist");

        var walletPath = GetWalletPath();
        await ImportWallet(walletPath, password);
    }

    public Task SaveWallet(string password)
    {
        var walletPath = GetWalletPath();
        var serializableWallet = MapWalletToSerializableWallet();
        var dataString = JsonConvert.SerializeObject(serializableWallet);
        var dataBytes = Encoding.UTF8.GetBytes(dataString);

        CryptoTools.Encrypt(dataBytes, walletPath, password);

        return Task.CompletedTask;
    }

    public Task ExportWallet(string exportPath, string password)
    {
        var serializableWallet = MapWalletToSerializableWallet();
        var dataString = JsonConvert.SerializeObject(serializableWallet);
        var dataBytes = Encoding.UTF8.GetBytes(dataString);

        CryptoTools.Encrypt(dataBytes, exportPath, password);

        return Task.CompletedTask;
    }

    public Task<List<Account>> ImportWallet(string importPath, string password)
    {
        var dataBytes = CryptoTools.Decrypt(importPath, password);
        var dataString = Encoding.UTF8.GetString(dataBytes);

        var serializableWallet = JsonConvert.DeserializeObject<SerializableWallet>(dataString);

        var wallet = _walletRepository.Get();
        var loadedAccounts = new List<Account>();
        foreach (var serializedAccount in serializableWallet.Accounts)
        {
            if (wallet.Accounts.Any(x => x.PublicPrivateKeyPair.PublicKey == serializedAccount.PublicKey))
                continue;

            var publicPrivateKeyPair = new PublicPrivateKeyPair(serializedAccount.PublicKey, _dataProtector.Protect(serializedAccount.UnprotectedPrivateKey));
            var walletAccount = new Account(publicPrivateKeyPair, serializedAccount.DisplayName);

            loadedAccounts.Add(walletAccount);
        }

        wallet.SetAccounts(loadedAccounts);

        return Task.FromResult(loadedAccounts);
    }

    private SerializableWallet MapWalletToSerializableWallet()
    {
        var wallet = _walletRepository.Get();
        var serializableWallet = new SerializableWallet
        {
            Accounts = wallet.Accounts.Select(x => new SerializableAccount
            {
                DisplayName = x.DisplayName,
                PublicKey = x.PublicPrivateKeyPair.PublicKey,
                UnprotectedPrivateKey = _dataProtector.Unprotect(x.PublicPrivateKeyPair.ProtectedPrivateKey)
            }).ToList()
        };

        return serializableWallet;
    }

    public string GetWalletPath()
    {
        return DEFAULT_WALLET_PATH;
    }

    private class SerializableWallet
    {
        public List<SerializableAccount> Accounts { get; set; }
    }

    private class SerializableAccount
    {
        public string DisplayName { get; set; }

        public string PublicKey { get; set; }

        public byte[] UnprotectedPrivateKey { get; set; }
    }
}