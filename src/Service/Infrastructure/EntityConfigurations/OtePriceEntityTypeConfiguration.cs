using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Model;
using System.Globalization;

namespace Service.Infrastructure.EntityConfigurations
{
    internal class OtePriceEntityTypeConfiguration : IEntityTypeConfiguration<OtePrice>
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm:ss";

        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        public void Configure(EntityTypeBuilder<OtePrice> builder)
        {
            builder.ToContainer("OtePrices");
            builder.HasNoDiscriminator();

            builder.Property(x => x.Id).ToJsonProperty("id");
            
            builder.Property(x => x.Date)
                .HasConversion(v => v.ToString(DateFormat, CultureInfo),
                v => ConvertDateOnlyFromString(v))
                .ToJsonProperty("date");
            
            builder.Property(x => x.Time).HasConversion(v => v.ToString(TimeFormat, CultureInfo),
                v => ConvertTimeOnlyFromString(v))
                .ToJsonProperty("time");

            builder.Property(x => x.Price).ToJsonProperty("price");

            //builder.HasPartitionKey(o => o.Date);

            builder.HasKey(x => x.Id);
        }

        private static DateOnly ConvertDateOnlyFromString(string input) => DateOnly.ParseExact(input, DateFormat, CultureInfo);
        private static TimeOnly ConvertTimeOnlyFromString(string input) => TimeOnly.ParseExact(input, TimeFormat, CultureInfo);
    }
}
