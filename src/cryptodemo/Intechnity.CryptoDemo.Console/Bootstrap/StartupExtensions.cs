using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Intechnity.CryptoDemo.Console.App;
using Intechnity.CryptoDemo.Console.Providers;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Core.Providers;
using Intechnity.CryptoDemo.Domain.Domain;
using Intechnity.CryptoDemo.Domain.Framework;
using Intechnity.CryptoDemo.Service.BackgroundServices;
using Intechnity.CryptoDemo.Service.DomainImplementations;
using Intechnity.CryptoDemo.Service.Framework;
using Intechnity.CryptoDemo.Service.Repositories;
using Intechnity.CryptoDemo.Service.Services;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Console.Bootstrap
{
    public static class StartupExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection @this)
        {
            @this.AddSingleton<ITranslationProvider, DummyTranslationProvider>();            
            @this.AddSingleton<IDateTimeProvider, RealDateTimeProvider>();
            @this.AddSingleton<IServerInfoProvider, KestrelServerInfoProvider>();

            @this.AddSingleton<IDomainBus, MediatorDomainBus>();
            @this.AddSingleton<IDataProtector, DefaultDataProtector>();
            @this.AddSingleton<IPublicPrivateKeyGenerator, RsaPublicPrivateKeyGenerator>();

            @this.AddSingleton<IBlockchainSeed, BlockchainSeed>();
            @this.AddSingleton<IWalletPersistor, WalletPersistor>();
            @this.AddSingleton<IP2PManager, P2PManager>();

            @this.AddSingleton<IAppState, AppState>();

            return @this;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection @this)
        {
            @this.AddSingleton<IAggregateRootRepository<Blockchain>, InMemoryAggregateRootRepository<Blockchain>>();
            @this.AddSingleton<IAggregateRootRepository<TransactionPool>, InMemoryAggregateRootRepository<TransactionPool>>();
            @this.AddSingleton<IAggregateRootRepository<Wallet>, InMemoryAggregateRootRepository<Wallet>>();

            return @this;
        }

        public static IServiceCollection RegisterDomain(this IServiceCollection @this)
        {
            @this.AddTransient<AggregateRootFactory>();
            return @this;
        }

        public static IServiceCollection RegisterHostedServices(this IServiceCollection @this)
        {
            @this.AddHostedService<AppHost>();
            @this.AddHostedService<MintingHostedService>();

            return @this;
        }

        public static IServiceCollection RegisterAutoMapper(this IServiceCollection @this)
        {
            var mapperConfiguration = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            @this.AddSingleton(mapperConfiguration.CreateMapper());

            return @this;
        }
    }
}