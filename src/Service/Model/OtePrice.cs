using Service.Interfaces;
using System.Globalization;

namespace Service.Model
{
    public record OtePrice(DateOnly Date, TimeOnly Time, decimal Price) : IEntity
    {
        public virtual string Id { get; set; } = $"{Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T{Time.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}.000Z";
    };
}
