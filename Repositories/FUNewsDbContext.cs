using BusinessObject;
using BusinessObject.Common;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class FUNewsDbContext : DbContext
    {
        public FUNewsDbContext(DbContextOptions<FUNewsDbContext> options)
            : base(options) { }

        public DbSet<SystemAccount> SystemAccounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NewsArticleTag> NewsArticleTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NewsArticleTag>().HasKey(nt => new { nt.NewsArticleId, nt.TagId });

            modelBuilder
                .Entity<NewsArticleTag>()
                .HasOne(nt => nt.NewsArticle)
                .WithMany(n => n.NewsArticleTags)
                .HasForeignKey(nt => nt.NewsArticleId);

            modelBuilder
                .Entity<NewsArticleTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NewsArticleTags)
                .HasForeignKey(nt => nt.TagId);

            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<NewsArticle>()
                .HasOne(n => n.CreatedBy)
                .WithMany(sa => sa.CreatedNewsArticles)
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<NewsArticle>()
                .HasOne(n => n.UpdatedBy)
                .WithMany(sa => sa.UpdatedNewsArticles)
                .HasForeignKey(n => n.UpdatedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
