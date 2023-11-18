namespace Service.Interfaces
{
    public interface IOteManager
    {
        Task GetOtePrices(DateOnly date);
    }
}
