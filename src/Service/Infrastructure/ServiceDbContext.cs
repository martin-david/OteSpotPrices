using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Service.Infrastructure.EntityConfigurations;
using Service.Model;
using System.Net;

namespace Service.Infrastructure
{
    public class ServiceDbContext : DbContext
    {
        private readonly IOptions<CosmosOptions> _cosmosOptions;

        public ServiceDbContext(DbContextOptions<ServiceDbContext> options,
            IOptions<CosmosOptions> cosmosOptions) : base(options)
        {
            _cosmosOptions = cosmosOptions;
        }

        public DbSet<OtePrice> OtePrices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseCosmos(_cosmosOptions.Value.ConnectionString,
                databaseName: _cosmosOptions.Value.DatabaseName,
                options =>
        {
            options.ConnectionMode(ConnectionMode.Direct);
            options.Region(Regions.WestEurope);
        });

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OtePriceEntityTypeConfiguration());
        }
    }
}
