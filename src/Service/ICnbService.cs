namespace OtePrices
{
    /// <summary>
    /// ICnbService
    /// </summary>
    public interface ICnbService
    {
        /// <summary>
        /// Gets the rate.
        /// Rate for a actual business date are published after 2:30PM CET time.
        /// For the next business date or actual business date before 2:30PM CET time we automatically using the last published rate.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<decimal> GetRate(DateOnly date, CancellationToken cancellationToken = default);
    }
}
