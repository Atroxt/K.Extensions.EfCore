using System.Linq.Expressions;
using K.Extensions.EfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace K.Extensions.EfCore.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void EnableSoftDeletionGlobalFilter(this ModelBuilder modelBuilder)
        {
            var entityTypesWithSoftDelete = modelBuilder
                .Model
                .GetEntityTypes()
                .Where(e => typeof(IAmSoftdeleteAble).IsAssignableFrom(e.ClrType));

            foreach (var entityType in entityTypesWithSoftDelete)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                var property = Expression.Property(parameter, nameof(IAmSoftdeleteAble.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
