using DotBoil.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotBoil.EFCore.Interceptors
{
    internal class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditUser _auditUser;

        public AuditInterceptor(IAuditUser auditUser)
        {
            _auditUser = auditUser;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var entities = eventData.Context.ChangeTracker.Entries<BaseEntity>();

            foreach (var entity in entities)
            {
                if(entity.State == EntityState.Added)
                {
                    entity.Entity.CreateUser = await _auditUser.GetModifierName();
                    entity.Entity.CreateTime = DateTime.Now;
                }else if(entity.State == EntityState.Modified)
                {
                    entity.Entity.ModifyUser = await _auditUser.GetModifierName();
                    entity.Entity.UpdateTime = DateTime.Now;
                }else if(entity.State == EntityState.Deleted)
                {
                    entity.Entity.ModifyUser = await _auditUser.GetModifierName();
                    entity.Entity.UpdateTime = DateTime.Now;
                    entity.Entity.IsDeleted = true;

                    entity.State = EntityState.Modified;
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
