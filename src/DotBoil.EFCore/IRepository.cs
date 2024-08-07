using DotBoil.Entities;

namespace DotBoil.EFCore
{
    public interface IRepository<TEntity, TContext>
        where TEntity : BaseEntity
        where TContext : EFCoreDbContext
    {
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        Task<TEntity> GetByIdAsync(int id);
        IQueryable<TEntity> Get();
        IQueryable<TEntity> GetAll();
        Task SaveChangesAsync();
    }
}
