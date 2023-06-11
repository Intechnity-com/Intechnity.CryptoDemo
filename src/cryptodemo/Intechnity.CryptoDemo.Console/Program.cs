using MediatR;
using Spectre.Console;
using System.Reflection;
using Intechnity.CryptoDemo.Console.Bootstrap;
using Intechnity.CryptoDemo.Core.Models.Configuration;
using Intechnity.CryptoDemo.Service.Services.P2P;

namespace Intechnity.CryptoDemo.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var mediatrAssemblies = Assembly.GetEntryAssembly()!.GetReferencedAssemblies().Where(x => x.FullName.Contains("CryptoDemo"))
                .Select(x => Assembly.Load(x))
                .ToArray();

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddCommandLine(args);

            builder.Logging
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Debug);

            builder.Services
                .Configure<BlockchainConfiguration>(builder.Configuration.GetSection(nameof(BlockchainConfiguration)))
                .RegisterDomain()
                .RegisterServices()
                .RegisterRepositories()
                .RegisterHostedServices()
                .RegisterAutoMapper()
                .AddMediatR(mediatrAssemblies)
                .AddGrpc();

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<P2PNodeEndpoint>();
                    endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client"); });
                });

            await app.RunAsync();
        }
    }
}