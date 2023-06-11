using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using System.Globalization;
using Intechnity.CryptoDemo.Core.Extensions;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Domain.Transactions;
using Intechnity.CryptoDemo.Domain.Protos;

namespace Intechnity.CryptoDemo.Console.Bootstrap
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Transaction, ProtoTransaction>()
                .ForMember(x => x.TransactionId, opt => opt.MapFrom(x => x.TransactionId.EmptyWhenNull()));
            CreateMap<ProtoTransaction, Transaction>()
                .ConvertUsing((source, target, ctx) =>
                {
                    var inputs = ctx.Mapper.Map<List<TransactionInput>>(source.TransactionInputs);
                    var outputs = ctx.Mapper.Map<List<TransactionOutput>>(source.TransactionOutputs);

                    return new Transaction(inputs, outputs);
                });

            CreateMap<TransactionInput, ProtoTransactionInput>()
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.FromAddress.EmptyWhenNull()))
                .ForMember(x => x.PreviousTransactionId, opt => opt.MapFrom(x => x.PreviousTransactionId.EmptyWhenNull()))
                .ForMember(x => x.Signature, opt => opt.MapFrom(x => x.Signature.EmptyWhenNull()))
                .ReverseMap();

            CreateMap<TransactionOutput, ProtoTransactionOutput>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Address.EmptyWhenNull()))
                .ForMember(x => x.Amount, opt => opt.MapFrom(x => x.Amount.ToString(CultureInfo.InvariantCulture)));
            CreateMap<ProtoTransactionOutput, TransactionOutput>()
                .ConvertUsing((source, target, ctx) =>
                {
                    return new TransactionOutput(
                        source.Address,
                        decimal.Parse(source.Amount, CultureInfo.InvariantCulture),
                        source.IsCoinbaseTransaction);
                });

            CreateMap<Block, ProtoBlock>()
                .ForMember(x => x.BlockchainId, opt => opt.MapFrom(x => x.BlockchainId.EmptyWhenNull()))
                .ForMember(x => x.Version, opt => opt.MapFrom(x => x.Version.EmptyWhenNull()))
                .ForMember(x => x.Timestamp, opt => opt.MapFrom(x => Timestamp.FromDateTimeOffset(x.Timestamp)))
                .ForMember(x => x.MinterAddress, opt => opt.MapFrom(x => x.MinterAddress.EmptyWhenNull()))
                .ForMember(x => x.MintingDifficulty, opt => opt.MapFrom(x => x.MintingDifficulty.ToString(CultureInfo.InvariantCulture)))
                .ForMember(x => x.PreviousHash, opt => opt.MapFrom(x => x.PreviousHash.EmptyWhenNull()))
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.Hash.EmptyWhenNull()));

            CreateMap<ProtoBlock, Block>()
                .ConvertUsing((source, target, ctx) =>
                {
                    var transactions = ctx.Mapper.Map<List<Transaction>>(source.Transactions);
                    return new Block(
                        source.BlockchainId,
                        source.Version,
                        source.Index,
                        source.Timestamp.ToDateTimeOffset(),
                        transactions,
                        decimal.Parse(source.MintingDifficulty, CultureInfo.InvariantCulture),
                        source.MinterAddress,
                        source.PreviousHash,
                        source.Hash);
                });
        }
    }
}