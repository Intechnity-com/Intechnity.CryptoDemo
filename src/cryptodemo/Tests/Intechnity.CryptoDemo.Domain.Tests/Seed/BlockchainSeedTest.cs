using FluentAssertions;
using Moq;
using Xunit;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Tests.Common.Framework;

namespace Intechnity.CryptoDemo.Domain.Tests.Seed
{
    public class BlockchainSeedTest : BaseDomainTests
    {
        [Fact]
        public void GetGenesisBlock_CanBeProperlyValidate()
        {
            var transactionPool = new TransactionPool(Mock.Of<IDomainBus>(), Mapper, DateTimeProvider, PublicPrivateKeyGenerator);

            var genesisBlock = BlockchainSeed.GetGenesisBlock(CryptoDemoConsts.GENESIS_BLOCK_MINTER_ADDRESS);

            var validationResult = genesisBlock.Validate(new List<Block>(), transactionPool, PublicPrivateKeyGenerator);
            validationResult.Should().BeTrue();
        }
    }
}