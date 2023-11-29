using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Service.Infrastructure.EntityConfigurations;
using Service.Model;
using System.Diagnostics.CodeAnalysis;

namespace Service.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class ServiceDbContext(DbContextOptions<ServiceDbContext> options,
        IOptions<CosmosOptions> cosmosOptions) : DbContext(options)
    {
        public DbSet<OtePrice> OtePrices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseCosmos(cosmosOptions.Value.ConnectionString,
                databaseName: cosmosOptions.Value.DatabaseName,
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
