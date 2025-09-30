using CISS411_GroupProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CISS411_GroupProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // This must be inside the class
        public DbSet<User> Users => Set<User>();

        // This must also be inside the class
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.UserID);
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(50);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
                e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("Visitor");
                e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending Confirmation");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
