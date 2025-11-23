using Microsoft.EntityFrameworkCore;
using proj1.Models;
using proj1.Constants;

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

            // Users (Passwords hashed with SHA256 of plain text shown below)
            // admin / admin = jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
            // user / user   = 6bq/p3MJUvWPqpRRjXv7k+8mXkLe0Y4E8NpGNfhYJac=
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@newsportal.com", PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", Role = Roles.Admin, CreatedAt = new DateTime(2024, 1, 1) },
                new User { Id = 2, Username = "user", Email = "user@newsportal.com", PasswordHash = "6bq/p3MJUvWPqpRRjXv7k+8mXkLe0Y4E8NpGNfhYJac=", Role = Roles.User, CreatedAt = new DateTime(2024, 1, 1) }
            );


            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology", IsActive = true },
                new Category { Id = 2, Name = "Sports", IsActive = true },
                new Category { Id = 3, Name = "Economy", IsActive = true }
            );

            // News
            modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Title = "Tech Boom", Content = "Technology is booming.", CategoryId = 1, IsPublished = true, PublishDate = new DateTime(2024, 1, 15), ImageUrl = "https://placehold.co/600x400" },
                new News { Id = 2, Title = "Sports Update", Content = "Team A won.", CategoryId = 2, IsPublished = true, PublishDate = new DateTime(2024, 1, 16), ImageUrl = "https://placehold.co/600x400" },
                new News { Id = 3, Title = "Economy Growth", Content = "Economy is growing.", CategoryId = 3, IsPublished = false, PublishDate = new DateTime(2024, 1, 17), ImageUrl = "https://placehold.co/600x400" }
            );
        }
    }
}
