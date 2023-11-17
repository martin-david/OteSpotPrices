using Microsoft.Extensions.DependencyInjection;
using OteCr;
using System.Text;

namespace OtePrices
{
    /// <summary>
    /// OteManager
    /// </summary>
    /// <seealso cref="OtePrices.IOteManager" />
    public class OteManager : IOteManager
    {
        private readonly ICnbService _cnbService;
        private readonly IOteCrService _oteCrService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OteManager"/> class.
        /// </summary>
        /// <param name="cnbService">The CNB service.</param>
        /// <param name="oteCrService">The ote cr service.</param>
        public OteManager([FromKeyedServices(nameof(CnbServiceCached))] ICnbService cnbService, IOteCrService oteCrService)
        {
            _cnbService = cnbService;
            _oteCrService = oteCrService;
        }

        /// <summary>
        /// Writes the Spot Market Index hourly by kWh in CZK for the date.
        /// </summary>
        /// <param name="date">The date.</param>
        public async Task GetOtePrices(DateOnly date)
        {
            var rateTask = _cnbService.GetRate(DateOnly.FromDateTime(DateTime.Today));
            var prices = await _oteCrService.GetDamPrice(date);

            Console.WriteLine($"Prices for date: {date:dd.MM.yyyy}");
            var rate = await rateTask;
            foreach (var price in prices)
            {
                decimal priceCalculated = Math.Round(price.Price * rate / 1000, 2, MidpointRounding.ToEven);
                Console.WriteLine($"{price.Hour} - {priceCalculated} CZK / kWh");
            }
        }

        /// <summary>
        /// Writes the market participants.
        /// </summary>
        /// <returns></returns>
        public async Task<GetRutListResponseResultRut[]> GetMarketParticipants()
        {
            var registeredMarketParticipants = await _oteCrService.GetRegisteredMarketParticipants();
            var orderedByName = registeredMarketParticipants.Rut.OrderBy(x => x.Company).ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var participant in orderedByName)
            {
                sb.AppendLine("ID - EAN - Name");
                sb.AppendLine($"{participant.Id} - {participant.Ean} - {participant.Company}");
            }

            Console.WriteLine(sb.ToString());

            return orderedByName;
        }
    }
}
