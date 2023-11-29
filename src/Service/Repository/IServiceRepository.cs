using Service.Interfaces;

namespace Service.Repository
{
    public interface IServiceRepository<TEntity> where TEntity : class, IEntity
    {
        IQueryable<TEntity> GetAll();

        Task<int> Create(IEnumerable<TEntity> entities);
    }
}
