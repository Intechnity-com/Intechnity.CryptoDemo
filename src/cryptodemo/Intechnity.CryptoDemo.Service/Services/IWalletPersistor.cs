using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Service.Services;

public interface IWalletPersistor
{
    bool CheckWalletExists();

    Task LoadWallet(string password);

    Task SaveWallet(string password);

    Task ExportWallet(string exportPath, string password);

    Task<List<Account>> ImportWallet(string importPath, string password);
}