using System.Diagnostics.CodeAnalysis;

namespace Service
{
    [ExcludeFromCodeCoverage]
    public sealed class CosmosOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
