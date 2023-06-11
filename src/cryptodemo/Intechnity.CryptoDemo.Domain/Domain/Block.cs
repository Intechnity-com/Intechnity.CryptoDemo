using System.Numerics;
using System.Text;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Tools;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;

namespace Intechnity.CryptoDemo.Domain.Domain;

public record Block
{
    public string BlockchainId { get; init; }

    public string Version { get; init; }

    public long Index { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public IReadOnlyCollection<Transaction> Transactions { get; init; }

    public decimal MintingDifficulty { get; init; }

    public string MinterAddress { get; init; }

    public string? PreviousHash { get; init; }

    public string Hash { get; init; }

    public Block(string blockchainId,
        string version,
        long index,
        DateTimeOffset timestamp,
        IEnumerable<Transaction> transactions,
        decimal mintingDifficulty,
        string minterAddress,
        string? previousHash)
    {
        BlockchainId = blockchainId;
        Version = version;
        Index = index;
        Timestamp = timestamp;
        Transactions = Array.AsReadOnly(transactions.ToArray());
        MintingDifficulty = mintingDifficulty;
        MinterAddress = minterAddress;
        PreviousHash = previousHash;
        Hash = CalculateHash();
    }

    public Block(string blockchainId,
        string version,
        long index,
        DateTimeOffset timestamp,
        IEnumerable<Transaction> transactions,
        decimal mintingDifficulty,
        string minterAddress,
        string previousHash,
        string hash)
    {
        BlockchainId = blockchainId;
        Version = version;
        Index = index;
        Timestamp = timestamp;
        Transactions = Array.AsReadOnly(transactions.ToArray());
        MintingDifficulty = mintingDifficulty;
        MinterAddress = minterAddress;
        PreviousHash = previousHash;
        Hash = hash;
    }

    private string CalculateHash()
    {
        var sb = new StringBuilder();
        sb.Append(BlockchainId);
        sb.Append(Version);
        sb.Append(Index);
        sb.Append(Timestamp.ToUnixTimeSeconds());

        foreach (var transaction in Transactions)
            sb.Append(transaction.TransactionId);

        sb.Append(MintingDifficulty);
        sb.Append(MinterAddress);
        sb.Append(PreviousHash);

        return HashTools.Sha256Hash(sb.ToString());
    }

    public bool Validate(IEnumerable<Block> previousBlocks, TransactionPool transactionPool, IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        if (BlockchainId != CryptoDemoConsts.BLOCKCHAIN_ID)
            return false;

        if (Version != CryptoDemoConsts.BLOCKCHAIN_VERSION)
            return false;

        if (Transactions.Count == 0)
            return false;

        if (Transactions.Any(x => !x.ValidateTransaction(previousBlocks, publicPrivateKeyGenerator)))
            return false;

        if (MintingDifficulty != CryptoDemoConsts.MINTING_DIFFICULTY)
            return false;

        if (Transactions.SelectMany(x => x.TransactionInputs).GroupBy(x => new { PreviousTransactionId = x.PreviousTransactionId, FromAddress = x.FromAddress }).Any(x => x.Count() > 1))
            return false; // for now we can't have the same transactionInput in multiple transactions in the same block -> logic doesn't handle such scenario

        var calculatedHash = CalculateHash();
        if (Hash != calculatedHash)
            return false;

        if (!ValidateStaking(transactionPool))
            return false;

        return true;
    }

    private bool ValidateStaking(TransactionPool transactionPool)
    {
        // Based on `SHA256(prevhash + address + timestamp) <= 2^256 * balance / diff

        BigInteger difficulty = CryptoDemoConsts.MINTING_DIFFICULTY;
        BigInteger balance = (BigInteger)transactionPool.FindBalanceForAddress(MinterAddress, Index);

        if (Index < CryptoDemoConsts.MAX_BLOCKS_WITHOUT_BALANCE_STAKING)
            balance += 50;

        var leftSideHash = HashTools.Sha256Bytes(PreviousHash + MinterAddress + Timestamp.ToUnixTimeSeconds());
        BigInteger leftSide = new BigInteger(leftSideHash, true, true);
        BigInteger rightSide = BigInteger.Multiply(BigInteger.Pow(2, 256), balance / difficulty);
        
        return leftSide <= rightSide;
    }
}