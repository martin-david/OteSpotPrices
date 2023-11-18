using OteCr;

namespace Service.Interfaces
{
    public interface IOteCrService
    {
        /// <summary>
        /// Gets the registered market participants.
        /// </summary>
        /// <param name="idFrom">The identifier from.</param>
        /// <param name="idTo">The identifier to.</param>
        /// <returns></returns>
        Task<GetRutListResponseResult> GetRegisteredMarketParticipants(int idFrom = 0, int idTo = 100000);

        /// <summary>
        /// Gets the Spot Market Index hourly in EUR for the date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        Task<GetDamPriceEResponseItem[]> GetDamPrice(DateOnly date);
    }
}