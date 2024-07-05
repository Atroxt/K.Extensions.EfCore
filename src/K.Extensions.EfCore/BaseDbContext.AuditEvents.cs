using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace K.Extensions.EfCore
{
    public partial class BaseDbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
