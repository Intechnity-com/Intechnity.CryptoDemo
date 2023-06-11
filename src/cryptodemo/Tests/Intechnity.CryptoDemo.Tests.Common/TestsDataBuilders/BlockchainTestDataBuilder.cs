using AutoMapper;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Extensions;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Helpers;
using Intechnity.CryptoDemo.Service.Services;

namespace Intechnity.CryptoDemo.Tests.Common.TestsDataBuilders
{
    public class BlockchainTestDataBuilder
    {
        private readonly IDomainBus _domainBus;
        private readonly IMapper _mapper;
        private readonly IBlockchainSeed _blockchainSeed;
        private readonly TransactionPool _transactionPool;
        private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

        private Blockchain _blockchain;

        public BlockchainTestDataBuilder(
            IDomainBus domainBus,
            IMapper mapper,
            IDateTimeProvider dateTimeProvider,
            IBlockchainSeed blockchainSeed,
            TransactionPool transactionPool,
            IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
        {
            _domainBus = domainBus;
            _mapper = mapper;
            _blockchainSeed = blockchainSeed;
            _transactionPool = transactionPool;
            _publicPrivateKeyGenerator = publicPrivateKeyGenerator;

            _blockchain = new Blockchain(domainBus, mapper, dateTimeProvider);
        }

        public BlockchainTestDataBuilder(
            IDomainBus domainBus,
            IMapper mapper,
            IBlockchainSeed blockchainSeed,
            TransactionPool transactionPool,
            IPublicPrivateKeyGenerator publicPrivateKeyGenerator,
            Blockchain blockchain)
        {
            _domainBus = domainBus;
            _mapper = mapper;
            _blockchainSeed = blockchainSeed;
            _transactionPool = transactionPool;
            _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
            _blockchain = blockchain;
        }

        public BlockchainTestDataBuilder SetGenesisBlock(string firstMintingAddress)
        {
            var genesisBlock = _blockchainSeed.GetGenesisBlock(firstMintingAddress);
            _blockchain.SetGenesisBlock(genesisBlock);

            _transactionPool.ProcessAllTransactions(_blockchain.Blocks);

            return this;
        }

        public BlockchainTestDataBuilder CreateAndAppendBlockWithTransaction(
            string fromAddress,
            string fromAddressKey,
            string toAddress,
            decimal amount,
            DateTimeOffset timestamp,
            string minterAddress,
            out Block newBlock)
        {
            CreateBlockWithTransaction(
                fromAddress,
                fromAddressKey,
                toAddress,
                amount,
                timestamp,
                minterAddress,
                out newBlock);

            _blockchain.ApppendExistingBlock(newBlock);

            _transactionPool.ProcessAllTransactions(_blockchain.Blocks);

            return this;
        }

        public BlockchainTestDataBuilder CreateBlockWithTransaction(
            string fromAddress,
            string fromAddressKey,
            string toAddress,
            decimal amount,
            DateTimeOffset timestamp,
            string minterAddress,
            out Block newblock)
        {
            var coinbaseTransaction = TransactionsHelper.CreateCoinbaseTransaction(minterAddress);
            var newTransaction = CreateTransaction(fromAddress, fromAddressKey, toAddress, amount);

            var lastBlock = _blockchain.Blocks.Last();

            newblock = new Block(
                CryptoDemoConsts.BLOCKCHAIN_ID,
                CryptoDemoConsts.BLOCKCHAIN_VERSION,
                lastBlock.Index + 1,
                timestamp,
                new List<Transaction> { coinbaseTransaction, newTransaction },
                CryptoDemoConsts.MINTING_DIFFICULTY,
                minterAddress,
                lastBlock.Hash);

            return this;
        }

        public BlockchainTestDataBuilder CreateBlockWithTransactions(
            List<Transaction> transactions,
            DateTimeOffset timestamp,
            string minterAddress,
            out Block newblock)
        {
            var lastBlock = _blockchain.Blocks.Last();

            var coinbaseTransaction = TransactionsHelper.CreateCoinbaseTransaction(minterAddress);
            var transactionsToInclude = new List<Transaction> { coinbaseTransaction };
            transactionsToInclude.AddRange(transactions);

            newblock = new Block(
                CryptoDemoConsts.BLOCKCHAIN_ID,
                CryptoDemoConsts.BLOCKCHAIN_VERSION,
                lastBlock.Index + 1,
                timestamp,
                transactionsToInclude,
                CryptoDemoConsts.MINTING_DIFFICULTY,
                minterAddress,
                lastBlock.Hash);

            _blockchain.ApppendExistingBlock(newblock);

            _transactionPool.ProcessAllTransactions(_blockchain.Blocks);

            return this;
        }

        public Transaction CreateTransaction(
            string fromAddress,
            string fromAddressKey,
            string toAddress,
            decimal amount)
        {
            var unspentTransactionOutputs = _transactionPool.UnspentTransactionOutputs.Where(x => x.Address == fromAddress).ToList();
            if (unspentTransactionOutputs == null)
                throw new InvalidOperationException();

            var newTransaction = TransactionsHelper.CreateUnsignedTransaction(fromAddress, unspentTransactionOutputs, toAddress, amount);
            foreach (var input in newTransaction.TransactionInputs)
                input.Sign(fromAddressKey.HexToBytes(), _publicPrivateKeyGenerator);

            return newTransaction;
        }

        public Blockchain Build()
        {
            return _blockchain;
        }
    }
}