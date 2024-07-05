using Microsoft.EntityFrameworkCore;

namespace K.Extensions.EfCore.Tests.InterceptorTests
{
    public class InterceptorDbContext : DbContext
    {
        public InterceptorDbContext(DbContextOptions<InterceptorDbContext> options) : base(options)
        { }

        public DbSet<TestEntitySoftDelete> TestEntitySoftDeletes { get; set; }
        public DbSet<TestEntitySoftDeleteAndDeletionTime>   TestEntitySoftDeleteAndDeletionTimes { get; set; }
        public DbSet<TestEntityCreationTime> TestEntityCreationTimes { get; set; }
        public DbSet<TestEntityModificationTime> TestEntityModificationTimes { get; set; }
    }
}
