using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWebsite1.Models;  // Ensure that the correct namespace is used

namespace MyWebsite1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Download> Downloads { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Photo)
                .WithMany()
                .HasForeignKey(c => c.PhotoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
