using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Intechnity.CryptoDemo.Core.Providers;

namespace Intechnity.CryptoDemo.Console.Providers;

public class KestrelServerInfoProvider : IServerInfoProvider
{
    private readonly IServer _server;

    public KestrelServerInfoProvider(IServer serverAddressesFeature)
    {
        _server = serverAddressesFeature;
    }

    public int GetListenPort()
    {
        var addressFeature = _server.Features.Get<IServerAddressesFeature>();
        var address = addressFeature!.Addresses.First();
        var uri = new Uri(address);

        return uri.Port;
    }
}
