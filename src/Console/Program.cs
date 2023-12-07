using Azure.Identity;
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
using Service.Repository;
using System.Net;

namespace OtePrices
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            // dotnet run k1=value1 -k2 value2 --alt3=value2 /alt4=value3 --alt5 value5 /alt6 value6
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline
            builder.Configuration.AddCommandLine(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment}.json", true, true)
                .AddUserSecrets<Program>(true, true);

            RegisterCosmosDbConnectionString(builder);

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

            builder.Services.AddSingleton(typeof(IServiceRepository<>), typeof(ServiceRepository<>));

            builder.Services.AddScoped<IOteManager, OteManager>();

            using IHost host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Host created.");

            var oteManager = host.Services.GetRequiredService<IOteManager>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            DateOnly startDate = configuration.GetValue<DateOnly>("startDate");
            DateOnly endDate = configuration.GetValue<DateOnly>("endDate");
            if (endDate == DateOnly.MinValue)
            {
                endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)); // we can obtain price for tomorrow at max
            }

            var otePrices = await oteManager.GetOtePrices(startDate, endDate);
            await oteManager.SaveOtePrices(otePrices);

            //var otePrices = await oteManager.LoadOtePrices();

            await host.RunAsync();
        }

        private static void RegisterCosmosDbConnectionString(IHostApplicationBuilder builder)
        {
            string cosmosDbConnectionString;

            if (builder.Environment.IsProduction())
            {
                string keyVaultName = builder.Configuration["AzureKeyVault:KeyVaultName"] ?? throw new ArgumentNullException("Missing configuration value for: KeyVaultName");

                builder.Configuration.AddAzureKeyVault(
                    new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());

                cosmosDbConnectionString = builder.Configuration["ConnectionString"] ?? throw new ArgumentNullException("Missing configuration value for: ConnectionString in Azure Key Vault.");
            }
            else
            {
                // Connection string loaded from User Secrets for development environment
                cosmosDbConnectionString = builder.Configuration["OteSpotPrices:ConnectionString"] ?? throw new ArgumentNullException("User Secrets is empty for OteSpotPrices:ConnectionString.");
            }

            builder.Services.AddOptions<CosmosOptions>()
                .BindConfiguration(nameof(CosmosOptions))
                .PostConfigure(cosmosOptions => cosmosOptions.ConnectionString = cosmosDbConnectionString);
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