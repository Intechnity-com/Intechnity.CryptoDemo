using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Service.Services;

public class AppState : IAppState
{
    public bool IsMinting { get; private set; }

    public Account? MintingAccount { get; private set; }

    public void StartMinting(Account account)
    {
        IsMinting = true;
        MintingAccount = account;
    }

    public void StopMinting()
    {
        IsMinting = false;
        MintingAccount = null;
    }
}