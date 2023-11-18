using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System.Globalization;
using System.Text;

namespace OtePrices
{
    /// <summary>
    /// CnbServiceCached
    /// </summary>
    /// <seealso cref="Service.Interfaces.ICnbService" />
    public class CnbServiceCached : ICnbService
    {
        private readonly ILogger<CnbServiceCached> _logger;
        private readonly IDistributedCache _cache;
        private readonly ICnbService _cnbService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CnbServiceCached"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="cnbService">The CNB service.</param>
        /// <exception cref="System.ArgumentNullException">cnbService</exception>
        public CnbServiceCached(ILogger<CnbServiceCached> logger, IDistributedCache cache,
            [FromKeyedServices(nameof(CnbService))] ICnbService cnbService)
        {
            _logger = logger;
            _cache = cache;
            _cnbService = cnbService ?? throw new ArgumentNullException(nameof(cnbService));
        }

        /// <summary>
        /// Gets the rate from cache or call the CnbService.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<decimal> GetRate(DateOnly date, CancellationToken cancellationToken = default)
        {
            string cacheKey = date.ToString();
            decimal rate;
            var value = await _cache.GetAsync(cacheKey, cancellationToken);
            if (value == null)
            {
                _logger.LogInformation($"Cache is null for cacheKey: {cacheKey}");
                rate = await _cnbService.GetRate(date, cancellationToken);
                byte[] data = Encoding.UTF8.GetBytes(rate.ToString(CultureInfo.InvariantCulture));
                await _cache.SetAsync(cacheKey, data);
            }
            else
            {
                _logger.LogInformation($"[{cacheKey}] {value}");
                string rateString = Encoding.UTF8.GetString(value);
                rate = decimal.Parse(rateString, CultureInfo.InvariantCulture);
            }

            return rate;
        }
    }
}
