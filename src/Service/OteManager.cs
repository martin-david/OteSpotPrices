using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OteCr;
using Service.Infrastructure;
using Service.Interfaces;
using Service.Model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace OtePrices
{
    /// <summary>
    /// OteManager
    /// </summary>
    /// <seealso cref="Service.Interfaces.IOteManager" />
    public class OteManager : IOteManager
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        private readonly ICnbService _cnbService;
        private readonly IOteCrService _oteCrService;
        private readonly ServiceDbContext _serviceDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OteManager" /> class.
        /// </summary>
        /// <param name="cnbService">The CNB service.</param>
        /// <param name="oteCrService">The ote cr service.</param>
        /// <param name="serviceDbContext">The service database context.</param>
        public OteManager([FromKeyedServices(nameof(CnbServiceCached))] ICnbService cnbService,
            IOteCrService oteCrService,
            ServiceDbContext serviceDbContext)
        {
            _cnbService = cnbService;
            _oteCrService = oteCrService;
            _serviceDbContext = serviceDbContext;
        }

        public async Task<List<OtePrice>> GetOtePrices(DateOnly startDate, DateOnly endDate)
        {
            DateOnly[] dates = GetDatesBetweenTwoDates(startDate, endDate);
            var result = new ConcurrentBag<OtePrice>();
            await Parallel.ForEachAsync(dates, new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            }, async (date, cancellationToken) =>
            {
                Console.WriteLine($"Processing date: {date}");
                var prices = await GetOtePrices(date);
                foreach (OtePrice price in prices)
                {
                    result.Add(price);
                }
            });

            var orderedResult = result.OrderBy(x => x.Date).ThenBy(x => x.Time).ToList();
            return orderedResult;
        }

        /// <summary>
        /// Writes the Spot Market Index hourly by kWh in CZK for the date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public async Task<List<OtePrice>> GetOtePrices(DateOnly date)
        {
            var rateTask = _cnbService.GetRate(date);
            var prices = await _oteCrService.GetDamPrice(date);

            Debug.WriteLine($"Prices for date: {date:dd.MM.yyyy}");
            var result = new List<OtePrice>();
            var rate = await rateTask;
            foreach (var price in prices)
            {
                decimal priceCalculated = Math.Round(price.Price * rate / 1000, 2, MidpointRounding.ToEven);
                result.Add(new OtePrice(date, new TimeOnly(price.Hour, 0), priceCalculated));
            }

            return result;
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

            Debug.WriteLine(sb.ToString());

            return orderedByName;
        }

        public async Task<List<OtePrice>> LoadOtePrices()
        {
            return await _serviceDbContext.OtePrices
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveOtePrices(IEnumerable<OtePrice> otePrices)
        {
            await _serviceDbContext.AddRangeAsync(otePrices);
            await _serviceDbContext.SaveChangesAsync();
        }

        private static DateOnly[] GetDatesBetweenTwoDates(DateOnly startDate, DateOnly endDate) => Enumerable.Range(0, 1 + (int)new DateTime(endDate, new TimeOnly(0, 0)).Subtract(new DateTime(startDate, new TimeOnly(0, 0))).TotalDays)
                .Select(startDate.AddDays)
                .ToArray();
    }
}
