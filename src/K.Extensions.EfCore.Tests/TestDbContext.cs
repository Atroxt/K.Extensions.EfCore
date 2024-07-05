using K.Extensions.EfCore.Extensions;
using K.Extensions.EfCore.Tests.Entities;
using Microsoft.EntityFrameworkCore;

namespace K.Extensions.EfCore.Tests
{
    internal class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        { }
        public DbSet<EntityWithSoftDelete> SoftDeleteEntities { get; set; }
        public DbSet<EntityWithSoftDeleteAndDeletedAt> SoftDeleteAndDeletedAtEntities { get; set; }
        public DbSet<EntityWithCreationTime> CreationTimeEntities { get; set; }
        public DbSet<EntityWithModificationTime> ModificationTimeEntities { get; set; }
        public DbSet<EntityWithAuditing> EntityWithAuditings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.EnableSoftDeletionGlobalFilter();
            base.OnModelCreating(modelBuilder);
        }
    }
}
