using Microsoft.EntityFrameworkCore;
using proj1.Models;

namespace proj1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<News>()
                .HasOne(n => n.Category)
                .WithMany(c => c.News)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint for User Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed Data
            // Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@newsportal.com", PasswordHash = "admin123", Role = "Admin", CreatedAt = DateTime.Now },
                new User { Id = 2, Username = "standard_user", Email = "user@newsportal.com", PasswordHash = "user123", Role = "User", CreatedAt = DateTime.Now }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology", IsActive = true },
                new Category { Id = 2, Name = "Sports", IsActive = true },
                new Category { Id = 3, Name = "Economy", IsActive = true }
            );

            // News
            modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Title = "Tech Boom", Content = "Technology is booming.", CategoryId = 1, IsPublished = true, PublishDate = DateTime.Now, ImageUrl = "https://placehold.co/600x400" },
                new News { Id = 2, Title = "Sports Update", Content = "Team A won.", CategoryId = 2, IsPublished = true, PublishDate = DateTime.Now, ImageUrl = "https://placehold.co/600x400" },
                new News { Id = 3, Title = "Economy Growth", Content = "Economy is growing.", CategoryId = 3, IsPublished = false, PublishDate = DateTime.Now, ImageUrl = "https://placehold.co/600x400" }
            );
        }
    }
}
