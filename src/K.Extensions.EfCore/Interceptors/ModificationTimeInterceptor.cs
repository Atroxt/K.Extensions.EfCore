using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace K.Extensions.EfCore.Interceptors;

public class ModificationTimeInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleModificationTime(eventData);
        return base.SavingChanges(eventData, result);
    }

    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        HandleModificationTime(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void HandleModificationTime(DbContextEventData eventData)
    {
        if (eventData.Context is null) return;
        var entities = eventData
            .Context
            .ChangeTracker
            .Entries<IHaveModificationTime>()
            .Where(e => e.State == EntityState.Modified);

        foreach (EntityEntry<IHaveModificationTime> entityEntry in entities)
        {
            entityEntry.Property(nameof(IHaveModificationTime.ModifiedAtUtc)).CurrentValue = DateTime.UtcNow;
        }
    }
}
