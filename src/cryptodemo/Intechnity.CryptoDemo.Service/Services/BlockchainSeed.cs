using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Helpers;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.Services;

public class BlockchainSeed : IBlockchainSeed
{
    private readonly IDomainBus _domainBus;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;

    public BlockchainSeed(
        IDomainBus domainBus,
        IAggregateRootRepository<Blockchain> blockchainRepository)
    {
        _domainBus = domainBus;
        _blockchainRepository = blockchainRepository;
    }

    public async Task SeedInitialBlockchainAsync(string minterAddress)
    {
        var genesisBlock = GetGenesisBlock(minterAddress);

        var blockchain = _blockchainRepository.Get();
        blockchain.SetGenesisBlock(genesisBlock);

        await _domainBus.RaiseImmediately(new LocalBlockchainStateChangedNotification());
    }

    public Block GetGenesisBlock(string minterAddress)
    {
        var transaction = TransactionsHelper.CreateCoinbaseTransaction(minterAddress);

        var block = new Block(
            CryptoDemoConsts.BLOCKCHAIN_ID,
            CryptoDemoConsts.BLOCKCHAIN_VERSION,
            0,
            CryptoDemoConsts.GENESIS_BLOCK_TIMESTAMP,
            new List<Transaction> { transaction },
            CryptoDemoConsts.MINTING_DIFFICULTY,
            minterAddress,
            null);

        return block;
    }
}