using Microsoft.Extensions.Logging;
using OteCr;

namespace OtePrices
{
    /// <summary>
    /// OteCrService
    /// </summary>
    /// <seealso cref="OtePrices.IOteCrService" />
    public class OteCrService : IOteCrService
    {
        private readonly ILogger<OteCrService> _logger;
        private readonly PublicDataServiceSoap _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="OteCrService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The client.</param>
        public OteCrService(ILogger<OteCrService> logger, PublicDataServiceSoap client)
        {
            _logger = logger;
            _client = client;
        }

        /// <summary>
        /// Gets the registered market participants.
        /// </summary>
        /// <param name="idFrom">The identifier from.</param>
        /// <param name="idTo">The identifier to.</param>
        /// <returns></returns>
        public async Task<GetRutListResponseResult> GetRegisteredMarketParticipants(int idFrom = 0, int idTo = 100000)
        {
            GetRutListResponseResult response = await _client.GetRutListAsync(idFrom, idTo);
            return response;
        }

        /// <summary>
        /// Gets the Spot Market Index hourly in EUR for the date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public async Task<GetDamPriceEResponseItem[]> GetDamPrice(DateOnly date)
        {
            var request = new GetDamPriceERequest
            {
                StartDate = new DateTime(date, default),
                StartHour = 0,
                EndDate = new DateTime(date, default),
                EndHour = 23,
                InEur = true
            };

            _logger.LogInformation("Calling OteCr service.");
            GetDamPriceEResponse response = await _client.GetDamPriceEAsync(request);
            return response.Result;
        }
    }
}
