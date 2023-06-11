using Intechnity.CryptoDemo.Domain.Domain;

namespace Intechnity.CryptoDemo.Service.Services
{
    public interface IBlockchainSeed
    {
        Task SeedInitialBlockchainAsync(string minterAddress);

        Block GetGenesisBlock(string minterAddress);
    }
}