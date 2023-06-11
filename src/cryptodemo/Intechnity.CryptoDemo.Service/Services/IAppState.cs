using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Service.Services;

public interface IAppState
{
    bool IsMinting { get; }

    Account? MintingAccount { get; }

    void StartMinting(Account account);

    void StopMinting();
}