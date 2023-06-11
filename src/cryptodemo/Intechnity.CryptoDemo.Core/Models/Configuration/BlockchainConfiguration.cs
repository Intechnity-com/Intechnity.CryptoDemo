namespace Intechnity.CryptoDemo.Core.Models.Configuration;

public class BlockchainConfiguration
{
    public string Id { get; set; } = "DEV";

    public List<string> KnownIpAddresses { get; set; } = new List<string>();
}