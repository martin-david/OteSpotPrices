using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System.Globalization;

namespace OtePrices
{
    /// <summary>
    /// CnbService
    /// </summary>
    /// <seealso cref="Service.Interfaces.ICnbService" />
    public class CnbService : ICnbService
    {
        private const string CnbRatesUrl = "https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt?date={0}";
        private const string DateFormat = "dd.MM.yyyy";
        private const string Eur = "EUR";
        private const char RowDelimiter = '\n'; //Environment.NewLine doesn't work here
        private const char ColumnDelimiter = '|';
        private static readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("cs-CZ");
        
        private readonly ILogger<CnbService> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CnbService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public CnbService(ILogger<CnbService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the rate.
        /// Rate for a actual business date are published after 2:30PM CET time.
        /// For the next business date or actual business date before 2:30PM CET time we automatically using the last published rate.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async virtual Task<decimal> GetRate(DateOnly date, CancellationToken cancellationToken = default)
        {
            string rates = await DownloadRates(date, cancellationToken);
            decimal eurRate = ParseEurRate(rates);
            return eurRate;
        }

        private async Task<string> DownloadRates(DateOnly date, CancellationToken cancellationToken = default)
        {
            string url = string.Format(CnbRatesUrl, date.ToString(DateFormat));
            _logger.LogInformation("Calling Cnb service.");
            return await _httpClient.GetStringAsync(url, cancellationToken);
        }

        private decimal ParseEurRate(string rates)
        {
            string eur = rates.Split(RowDelimiter)
                .Skip(2) // skip first two header rows
                .Single(x => x.Contains(Eur))
                .Split(ColumnDelimiter)
                .Last(); // value is in the last column

            return decimal.Parse(eur, CultureInfo);
        }
    }
}
