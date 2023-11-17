namespace OtePrices
{
    public interface IOteManager
    {
        Task GetOtePrices(DateOnly date);
    }
}
