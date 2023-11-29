using Microsoft.EntityFrameworkCore;
using Service.Infrastructure;
using Service.Interfaces;

namespace Service.Repository
{
    public class ServiceRepository<TEntity>(ServiceDbContext context)
        : IServiceRepository<TEntity> where TEntity : class, IEntity
    {
        public IQueryable<TEntity> GetAll()
        {
            return context.Set<TEntity>().AsNoTracking();
        }

        public async Task<int> Create(IEnumerable<TEntity> entities)
        {
            await context.Set<TEntity>().AddRangeAsync(entities);
            return await context.SaveChangesAsync();
        }
    }
}
