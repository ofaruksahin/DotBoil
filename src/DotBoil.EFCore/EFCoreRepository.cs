using DotBoil.Entities;
using Microsoft.EntityFrameworkCore;
namespace DotBoil.EFCore
{
    public class EFCoreRepository<TEntity, TContext> : IRepository<TEntity, TContext>
        where TEntity : BaseEntity
        where TContext : EFCoreDbContext
    {
        protected TContext _context;

        public EFCoreRepository(
            TContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().UpdateRange(entities);
        }

        public void Remove(TEntity entity)
        {
            entity.IsDeleted = true;
            _context.Set<TEntity>().Update(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var baseEntity in entities)
                baseEntity.IsDeleted = true;

            _context.Set<TEntity>().UpdateRange(entities);
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _context
                .Set<TEntity>()
                .AsNoTracking()
                .Where(entity => entity.Id == id && !entity.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public IQueryable<TEntity> Get()
        {
            return GetQueryable();
        }

        public IQueryable<TEntity> GetAll()
        {
            return GetQueryable();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        private IQueryable<TEntity> GetQueryable()
        {
            return _context
              .Set<TEntity>()
              .Where(entity => !entity.IsDeleted)
              .AsNoTracking()
              .AsQueryable();
        }
    }
}
