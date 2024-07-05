using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace K.Extensions.EfCore.Models
{
    internal class AuditEntry
    {
        public EntityEntry Entry { get; }
        private string EntityName { get; set; }
        private string EntityId { get; set; }
        private DateTime Timestamp { get; set; }
        private string Operation { get; set; }
        private Dictionary<string, object> Changes { get; } = new Dictionary<string, object>();

        public AuditEntry(EntityEntry entry)
        {
            Entry=entry;
            EntityName = entry.Entity.GetType().Name;
            Timestamp=DateTime.UtcNow;
            switch (entry.State)
            {
                case EntityState.Added:
                    Operation = "Added";
                    break;
                case EntityState.Modified:
                    Operation = "Modified";
                    break;
                case EntityState.Deleted:
                    Operation = "Deleted";
                    break;
            }
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (entry.State == EntityState.Added || property.IsModified)
                {
                    Changes[propertyName] = property.CurrentValue;
                }
            }

            // Bestimme die Entity ID
            var key = entry.Properties.First(p => p.Metadata.IsPrimaryKey());
            EntityId = key.CurrentValue?.ToString();
        }
        public AuditLog ToAuditLog()
        {
            return new AuditLog
            {
                EntityName = EntityName,
                EntityId = EntityId,
                Operation = Operation,
                Changes = JsonSerializer.Serialize(Changes),
                Timestamp = Timestamp
            };
        }
    }
}
