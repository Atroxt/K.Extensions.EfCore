using FluentAssertions;
using K.Extensions.EfCore.Interceptors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace K.Extensions.EfCore.Tests.InterceptorTests
{
    [TestClass]
    public class InterceptorTests
    {
        private InterceptorDbContext context = default!;

        [TestInitialize]
        public void Setup()
        {
            var sp = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<SoftDeleteInterceptor>()
                .AddSingleton<ModificationTimeInterceptor>()
                .AddSingleton<CreationTimeInterceptor>()
                .BuildServiceProvider();
            var options = new DbContextOptionsBuilder<InterceptorDbContext>()
                .UseInMemoryDatabase("TestDb", databaseRoot: new InMemoryDatabaseRoot())
                .AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<ModificationTimeInterceptor>(),
                    sp.GetRequiredService<CreationTimeInterceptor>()
                    )
                .UseInternalServiceProvider(sp)
                .EnableSensitiveDataLogging()
                .Options;
            this.context = new InterceptorDbContext(options);
        }

        [TestMethod]
        public async Task RemoveEntryWithSoftdelete_ShouldBeSoftDeleted()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new TestEntitySoftDelete() { Id = id };
            context.Add(data);
            await context.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();

            context.Remove(data);
            await context.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task RemoveEntryWithSoftdeleteAndDeletionTime_ShouldBeSoftDeleted()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new TestEntitySoftDeleteAndDeletionTime() { Id = id };
            context.Add(data);
            await context.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeFalse();
            data.DeletedAtUtc.Should().BeNull();

            context.Remove(data);
            await context.SaveChangesAsync(ct);
            data.IsDeleted.Should().BeTrue();
            data.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [TestMethod]
        public async Task AddEntryWithCreationTime_ShouldBeAddedCreationTime()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new TestEntityCreationTime() { Id = id };
            context.Add(data);
            await context.SaveChangesAsync(ct);
            data.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [TestMethod]
        public async Task UpdateEntryWithModificationTime_ShoulHaveModificationTime()
        {
            var ct = new CancellationToken();
            var id = Guid.NewGuid();
            var data = new TestEntityModificationTime() { Id = id, Name = "Added" };
            context.Add(data);
            await context.SaveChangesAsync(ct);
            data.ModifiedAtUtc.Should().BeNull();

            data.Name = "Modify";
            await context.SaveChangesAsync(ct);
            data.ModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }
    }
}
