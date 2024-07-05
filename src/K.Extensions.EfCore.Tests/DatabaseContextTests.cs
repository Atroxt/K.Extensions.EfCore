using FluentAssertions;
using K.Extensions.EfCore.Tests.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace K.Extensions.EfCore.Tests
{
    [TestClass]
    public class DatabaseContextTests
    {
        private TestDbContext ctx = default!;

        [TestInitialize]
        public void Initialize()
        {
            var sp = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestDb", databaseRoot: new InMemoryDatabaseRoot())
                .UseInternalServiceProvider(sp)
                .EnableSensitiveDataLogging()
                .Options;
            this.ctx = new TestDbContext(options);
        }

        [TestMethod]
        public async Task RemoveEntity_WithSoftDelete_ShouldHaveDeleted()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new EntityWithSoftDelete() { Id = id };
            ctx.Add(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();

            ctx.Remove(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task RemoveEntity_WithSoftDelete_WithDeletedAt_ShouldHaveDeletedAndDeletedAt()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new EntityWithSoftDeleteAndDeletedAt() { Id = id };
            ctx.Add(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();
            data.DeletedAtUtc.Should().BeNull();

            ctx.Remove(data);
            await ctx.SaveChangesAsync(ct);
            data.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [TestMethod]
        public async Task AddEntity_WithCreatedAt_ShouldHaveCreatedAt()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new EntityWithCreationTime() { Id = id };
            ctx.Add(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();
            data.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task ModifyEntity_WithModifiedAt_ShouldHaveModifiedAt()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new EntityWithModificationTime() { Id = id, Description = "Added" };
            ctx.Add(data);

            await ctx.SaveChangesAsync(ct);
            data.ModifiedAtUtc.Should().BeNull();

            data.Description = "Modification";
            await ctx.SaveChangesAsync(ct);
            data.ModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task SoftDeletedEntity_ShouldNotAbleToGetFromContext()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new EntityWithCreationTime() { Id = id };
            ctx.Add(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();

            ctx.Remove(data);
            await ctx.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeTrue();

            var avD = await ctx.CreationTimeEntities.FirstOrDefaultAsync(p => p.Id == id, ct);
            avD.Should().BeNull();
        }

        [TestMethod]
        public async Task SaveChanges_ShouldCreateAuditLog_ForAddedEntity()
        {
            var entity = new EntityWithAuditing { Name = "Test" };
            ctx.Add(entity);

            await ctx.SaveChangesAsync();

            var auditLog = ctx.AuditLogs.FirstOrDefault();
            auditLog.Should().NotBeNull();
            auditLog!.EntityName.Should().Be(nameof(EntityWithAuditing));
            auditLog.Operation.Should().Be("Added");
            auditLog.Changes.Should().Contain("Name", "Test");
        }

        [TestMethod]
        public void SaveChanges_ShouldCreateAuditLog_ForModifiedEntity()
        {

            var entity = new EntityWithAuditing { Name = "Test" };
            ctx.Add(entity);
            ctx.SaveChanges();

            entity.Name = "Updated Test";
            ctx.Update(entity);

            ctx.SaveChanges();

            var auditLog = ctx.AuditLogs.OrderByDescending(a => a.Timestamp).FirstOrDefault();
            auditLog.Should().NotBeNull();
            auditLog!.EntityName.Should().Be(nameof(EntityWithAuditing));
            auditLog.Operation.Should().Be("Modified");
            auditLog.Changes.Should().Contain("Name", "Updated Test");
        }

        [TestMethod]
        public void SaveChanges_ShouldCreateAuditLog_ForDeletedEntity()
        {
            var entity = new EntityWithAuditing { Name = "Test" };
            ctx.Add(entity);
            ctx.SaveChanges();

            ctx.Remove(entity);

            ctx.SaveChanges();

            var auditLog = ctx.AuditLogs.OrderByDescending(a => a.Timestamp).FirstOrDefault();
            auditLog.Should().NotBeNull();
            auditLog!.EntityName.Should().Be(nameof(EntityWithAuditing));
            auditLog.Operation.Should().Be("Deleted");
        }
    }
}