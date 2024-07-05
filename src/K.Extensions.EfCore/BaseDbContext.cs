using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace K.Extensions.EfCore
{
    public partial class BaseDbContext : DbContext
    {
        public BaseDbContext() { }

        public BaseDbContext(DbContextOptions options) : base(options)
        { }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var auditEntries = OnBeforeSaveChanges();
            HandleSoftDeletes();
            HandleCreationTime();
            HandleModifiedTime();

            var result = await base.SaveChangesAsync(cancellationToken);
            OnAfterSaveChanges(auditEntries);

            return result;
        }

        public override int SaveChanges()
        {
            return this.SaveChangesAsync(CancellationToken.None).Result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            var entities = this.ChangeTracker
                .Entries<IAmAuditAble>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);

            return entities.Select(entityEntry => new AuditEntry(entityEntry)).ToList();
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            base.SaveChanges();
        }
   
        private void HandleModifiedTime()
        {
            var entities = this.ChangeTracker
                .Entries<IHaveModificationTime>()
                .Where(e => e.State == EntityState.Modified);

            foreach (EntityEntry<IHaveModificationTime> entityEntry in entities)
            {
                entityEntry.Property(nameof(IHaveModificationTime.ModifiedAtUtc)).CurrentValue = DateTime.UtcNow;
            }
        }

        private void HandleCreationTime()
        {
            var entities = this.ChangeTracker
                .Entries<IHaveCreationTime>()
                .Where(e => e.State == EntityState.Added);

            foreach (EntityEntry<IHaveCreationTime> entityEntry in entities)
            {
                entityEntry.Property(nameof(IHaveCreationTime.CreatedAtUtc)).CurrentValue = DateTime.UtcNow;
            }
        }

        private void HandleSoftDeletes()
        {
            var entities = this.ChangeTracker
                .Entries<IAmSoftdeleteAble>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (EntityEntry<IAmSoftdeleteAble> entityEntry in entities)
            {
                entityEntry.State = EntityState.Modified;
                entityEntry.Property(nameof(IAmSoftdeleteAble.IsDeleted)).CurrentValue = true;

                if (entityEntry.Entity is IHaveDeletionTime deletionTimeEntity)
                {
                    entityEntry.Property(nameof(IHaveDeletionTime.DeletedAtUtc)).CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
