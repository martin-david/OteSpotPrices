using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OteCr;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Service;
using Service.Infrastructure;
using Service.Interfaces;
using System.Net;

namespace OtePrices
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment}.json", true, true)
                .AddUserSecrets<Program>(true, true);

            builder.Services.AddOptions<CosmosOptions>()
                .BindConfiguration(nameof(CosmosOptions))
                .PostConfigure(cosmosOptions => cosmosOptions.ConnectionString = builder.Configuration["OteSpotPrices:ConnectionString"] ?? throw new ArgumentNullException("User Secrets is empty for OteSpotPrices:ConnectionString."));

            builder.Services.AddLogging();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpClient<ICnbService, CnbService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            builder.Services.AddDbContext<ServiceDbContext>();

            // Connected service (WCF) registration
            builder.Services.AddScoped<PublicDataServiceSoap>(factory =>
            {
                var client = new PublicDataServiceSoapClient();
                return client;
            });

            builder.Services.AddKeyedScoped<ICnbService, CnbService>(nameof(CnbService));
            builder.Services.AddKeyedScoped<ICnbService, CnbServiceCached>(nameof(CnbServiceCached));
            builder.Services.AddScoped<IOteCrService, OteCrService>();

            builder.Services.AddScoped<IOteManager, OteManager>();

            using IHost host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Host created.");

            var oteManager = host.Services.GetRequiredService<IOteManager>();

            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 11, 28);
            var otePrices = await oteManager.GetOtePrices(startDate, endDate);
            await oteManager.SaveOtePrices(otePrices);

            //var otePrices = await oteManager.LoadOtePrices();

            await host.RunAsync();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var sleepDurations = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout || msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(sleepDurations);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() => HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}