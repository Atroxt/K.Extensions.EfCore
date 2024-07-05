using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace K.Extensions.EfCore.Interceptors
{
    public class CreationTimeInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            HandleCreationTime(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            HandleCreationTime(eventData);
            return base.SavingChanges(eventData, result);
        }

        private static void HandleCreationTime(DbContextEventData eventData)
        {
            if (eventData.Context is null) return;
            var entities = eventData
                .Context
                .ChangeTracker
                .Entries<IHaveCreationTime>()
                .Where(e => e.State == EntityState.Added);

            foreach (EntityEntry<IHaveCreationTime> entityEntry in entities)
            {
                entityEntry.Property(nameof(IHaveCreationTime.CreatedAtUtc)).CurrentValue = DateTime.UtcNow;
            }
        }

        
    }
}
