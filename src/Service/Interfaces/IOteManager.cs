using Service.Model;

namespace Service.Interfaces
{
    /// <summary>
    /// IOteManager
    /// </summary>
    public interface IOteManager
    {
        /// <summary>
        /// Gets the ote prices.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        Task<List<OtePrice>> GetOtePrices(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Gets the ote prices.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        Task<List<OtePrice>> GetOtePrices(DateOnly date);

        /// <summary>
        /// Loads the ote prices.
        /// </summary>
        /// <returns></returns>
        Task<List<OtePrice>> LoadOtePrices();

        /// <summary>
        /// Saves the ote prices.
        /// </summary>
        /// <param name="otePrices">The ote prices.</param>
        /// <returns></returns>
        Task SaveOtePrices(IEnumerable<OtePrice> otePrices);
    }
}
