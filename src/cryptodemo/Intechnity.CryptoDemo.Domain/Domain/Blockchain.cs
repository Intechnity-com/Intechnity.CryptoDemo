using AutoMapper;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Domain.Domain;

public class Blockchain : AggregateRoot, IApply<AppendMinedBlockToBlockchainNotification>, IApply<ApplySyncedBlocksNotification>
{
    private LinkedList<Block> _blocks = new LinkedList<Block>();

    public Blockchain(IDomainBus bus, IMapper mapper, IDateTimeProvider dateTimeProvider)
        : base(bus, mapper, dateTimeProvider)
    {
    }

    public IReadOnlyCollection<Block> Blocks => _blocks;

    public void SetGenesisBlock(Block block)
    {
        _blocks.AddFirst(block);
    }

    public void ApppendExistingBlock(Block block)
    {
        _blocks.AddLast(block);
    }

    public Block? GetBlock(long index)
    {
        if (index - 1 > _blocks.Count)
            return null;

        return _blocks.First(x => x.Index == index);
    }

    public async Task ValidateAndAppendMyMinedBlock(Block block,
        TransactionPool transactionPool,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        if (!ValidateMinedBlock(block, Blocks, transactionPool, publicPrivateKeyGenerator, dateTimeProvider))
            throw new InvalidOperationException("Tried to append invalid block!");

        var blockDTO = Mapper.Map<ProtoBlock>(block);

        var @event = new AppendMinedBlockToBlockchainNotification(blockDTO, true);
        await RaiseEvent(@event);
    }

    public async Task ValidateAndAppendDistributedMinedBlock(Block block,
        TransactionPool transactionPool,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        if (!ValidateMinedBlock(block, Blocks, transactionPool, publicPrivateKeyGenerator, DateTimeProvider))
            throw new InvalidOperationException("Tried to append invalid block!");

        var blockDTO = Mapper.Map<ProtoBlock>(block);

        var @event = new AppendMinedBlockToBlockchainNotification(blockDTO, false);
        await RaiseEvent(@event);
    }

    public void Apply(AppendMinedBlockToBlockchainNotification @event)
    {
        var block = Mapper.Map<Block>(@event.Block);
        _blocks.AddLast(block);
    }

    public async Task ValidateAndSetBlocksFromBlockchainSync(IReadOnlyList<Block> newBlocks,
        TransactionPool transactionPool,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        var minIndex = newBlocks.Min(x => x.Index);

        newBlocks = newBlocks.OrderBy(x => x.Index).ToList();
        var previousBlocks = _blocks.Where(x => x.Index < minIndex).ToList();

        foreach (var newBlock in newBlocks)
        {
            if (!ValidateBlock(newBlock, previousBlocks, transactionPool, publicPrivateKeyGenerator))
                throw new InvalidDataException("Block validation failed!");

            previousBlocks.Add(newBlock);
        }

        var blockDTOs = Mapper.Map<IReadOnlyList<ProtoBlock>>(newBlocks);
        await RaiseEvent(new ApplySyncedBlocksNotification(blockDTOs));
    }

    public void Apply(ApplySyncedBlocksNotification @event)
    {
        var blocks = Mapper.Map<IReadOnlyList<Block>>(@event.SyncedBlocks);

        var firstBlockIndexToAdd = blocks.First().Index;
        while (_blocks.Count > firstBlockIndexToAdd)
            _blocks.RemoveLast();

        foreach (var block in blocks)
        {
            _blocks.AddLast(block);
        }
    }

    public bool ValidateMinedBlock(Block block,
        IEnumerable<Block> previousBlocks,
        TransactionPool transactionPool,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        var lastBlock = _blocks.Last!.Value;

        if (block.Index != lastBlock.Index + 1)
            return false;

        var now = dateTimeProvider.Now;
        var differenceInMinutes = Math.Abs((now - block.Timestamp).TotalMinutes);
        if (differenceInMinutes > CryptoDemoConsts.MAX_CLOCK_DIFF.TotalMinutes)
            return false;

        return ValidateBlock(block, previousBlocks, transactionPool, publicPrivateKeyGenerator);
    }

    public bool ValidateBlock(Block block,
        IEnumerable<Block> previousBlocks,
        TransactionPool transactionPool,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        var previousBlock = previousBlocks.Last(x => x.Index == block.Index - 1);

        if (block.PreviousHash != previousBlock.Hash)
            return false;

        var minutesBetweenBlocks = Math.Abs((block.Timestamp.UtcDateTime - previousBlock.Timestamp.UtcDateTime).TotalMinutes);
        if (minutesBetweenBlocks < CryptoDemoConsts.MIN_BLOCK_TIMESTAMP_DIFF.TotalMinutes)
            return false;

        var futureLimit = DateTimeProvider.Now.Add(CryptoDemoConsts.MAX_CLOCK_DIFF);
        if (block.Timestamp.UtcDateTime > futureLimit.UtcDateTime)
            return false;

        return block.Validate(previousBlocks, transactionPool, publicPrivateKeyGenerator);
    }
}