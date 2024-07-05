using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace K.Extensions.EfCore.Interceptors
{
    public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            HandleSoftDelete(eventData);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            HandleSoftDelete(eventData);
            return base.SavingChanges(eventData, result);
        }

        private static void HandleSoftDelete(DbContextEventData eventData)
        {
            if (eventData.Context is null) return;
            var entities = eventData
                .Context
                .ChangeTracker
                .Entries<IAmSoftdeleteAble>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (EntityEntry<IAmSoftdeleteAble> entityEntry in entities)
            {
                entityEntry.State = EntityState.Modified;
                entityEntry.Property(nameof(IAmSoftdeleteAble.IsDeleted)).CurrentValue = true;
                if (entityEntry.Entity is IHaveDeletionTime)
                {
                    entityEntry.Property(nameof(IHaveDeletionTime.DeletedAtUtc)).CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
