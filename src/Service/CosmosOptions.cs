namespace Service
{
    public sealed class CosmosOptions
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
    }
}
