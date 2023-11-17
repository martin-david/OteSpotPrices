using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OteCr;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System.Net;

namespace OtePrices
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpClient<ICnbService, CnbServiceCached>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            
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

            var otePrices = host.Services.GetRequiredService<IOteManager>();
            await otePrices.GetOtePrices(DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

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