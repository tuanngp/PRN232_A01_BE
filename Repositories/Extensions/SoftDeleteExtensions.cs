using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Common;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;

namespace Repositories.Extensions
{
    public static class SoftDeleteExtensions
    {
        public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query)
            where T : class, ISoftDelete
        {
            return query;
        }

        public static IQueryable<T> ExcludeDeleted<T>(this IQueryable<T> query)
            where T : class, ISoftDelete
        {
            return query.Where(e => !e.IsDeleted);
        }

        public static async Task SoftDeleteAsync<T>(
            this IGenericRepository<T> repository,
            T entity,
            int userId
        )
            where T : class, ISoftDelete
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedById = userId;

            // Entity is updated, not removed from database
            await Task.CompletedTask;
        }

        public static async Task RestoreAsync<T>(this IGenericRepository<T> repository, T entity)
            where T : class, ISoftDelete
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedById = null;

            // Entity is updated, not removed from database
            await Task.CompletedTask;
        }

        public static void ApplySoftDeleteFilter<T>(this ModelBuilder modelBuilder)
            where T : class, ISoftDelete
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
