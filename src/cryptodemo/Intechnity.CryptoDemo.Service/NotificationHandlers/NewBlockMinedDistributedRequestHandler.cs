using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Events;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.Framework;

namespace Intechnity.CryptoDemo.Service.NotificationHandlers;

public class NewBlockMinedDistributedRequestHandler : INotificationHandler<NewBlockMinedDistributedNotification>
{
    private readonly ILogger<NewBlockMinedDistributedRequestHandler> _logger;
    private readonly IDomainBus _domainBus;
    private readonly IMapper _mapper;
    private readonly IAggregateRootRepository<Blockchain> _blockchainRepository;
    private readonly IAggregateRootRepository<TransactionPool> _transactionPoolRepository;
    private readonly IPublicPrivateKeyGenerator _publicPrivateKeyGenerator;

    public NewBlockMinedDistributedRequestHandler(
        ILogger<NewBlockMinedDistributedRequestHandler> logger,
        IDomainBus domainBus,
        IMapper mapper,
        IAggregateRootRepository<Blockchain> blockchainRepository,
        IAggregateRootRepository<TransactionPool> transactionPoolRepository,
        IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        _logger = logger;
        _domainBus = domainBus;
        _mapper = mapper;
        _blockchainRepository = blockchainRepository;
        _transactionPoolRepository = transactionPoolRepository;
        _publicPrivateKeyGenerator = publicPrivateKeyGenerator;
    }

    public async Task Handle(NewBlockMinedDistributedNotification request, CancellationToken cancellationToken)
    {
        try
        {
            var myBlockchain = _blockchainRepository.Get();
            var myTransactionPool = _transactionPoolRepository.Get();

            var block = _mapper.Map<Block>(request.Block);
            await myBlockchain.ValidateAndAppendDistributedMinedBlock(block, myTransactionPool, _publicPrivateKeyGenerator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append distributed mined block");
        }
    }
}