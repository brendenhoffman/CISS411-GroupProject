using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CISS411_GroupProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CISS411_GroupProject.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> AppUsers => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Design> Designs => Set<Design>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<EmployeeAssignment> EmployeeAssignments => Set<EmployeeAssignment>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            // USERS
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.UserID);
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(50);
                e.Property(x => x.Address).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
                e.Property(x => x.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Visitor");
                e.Property(x => x.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Pending Confirmation");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.UpdatedAt).IsRequired(false);
                e.Property(x => x.IdentityUserId).IsRequired().HasMaxLength(450);
                e.HasOne(x => x.IdentityUser)
                 .WithMany()
                 .HasForeignKey(x => x.IdentityUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Check constraints for Role/Status values (you can relax these if you want to allow more later)
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Users_Role",
                      "Role IN ('Visitor','Customer','Employee','Admin')");
                    tb.HasCheckConstraint("CK_Users_Status",
                      "Status IN ('Pending Confirmation','Active','Inactive')");
                });
            });

            // ORDERS
            b.Entity<Order>(e =>
            {
                e.HasKey(x => x.OrderID);
                e.Property(x => x.Occasion).IsRequired().HasMaxLength(50);
                e.Property(x => x.DeliveryDate).IsRequired();
                e.Property(x => x.Budget).HasColumnType("decimal(10,2)");
                e.Property(x => x.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Pending");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.UpdatedAt).IsRequired(false);

                e.HasOne(x => x.Customer)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(x => x.CustomerID)
                  .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Orders_Status",
                      "Status IN ('Pending','Awaiting Customer Approval','In Process','Ready','Picked Up')");
                });
            });

            // ORDER ITEMS
            b.Entity<OrderItem>(e =>
            {
                e.HasKey(x => x.OrderItemID);
                e.Property(x => x.ItemName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Quantity).IsRequired();

                e.Property(x => x.CustomDescription).HasMaxLength(500).IsRequired(false);
                e.Property(x => x.DesignApproved).HasDefaultValue(false).IsRequired();

                e.HasOne(x => x.Order)
                 .WithMany(o => o.OrderItems)
                 .HasForeignKey(x => x.OrderID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // DESIGNS
            b.Entity<Design>(e =>
            {
                e.HasKey(x => x.DesignID);
                e.Property(x => x.ImagePath).IsRequired().HasMaxLength(255);
                e.Property(x => x.UploadedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Order)
                  .WithMany(o => o.Designs)
                  .HasForeignKey(x => x.OrderID)
                  .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Employee)
                  .WithMany(u => u.Designs)
                  .HasForeignKey(x => x.EmployeeID)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            // FEEDBACK
            b.Entity<Feedback>(e =>
            {
                e.HasKey(x => x.FeedbackID);
                e.Property(x => x.FeedbackText).IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Order)
                  .WithMany(o => o.Feedbacks)
                  .HasForeignKey(x => x.OrderID)
                  .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Customer)
                  .WithMany(u => u.Feedbacks)
                  .HasForeignKey(x => x.CustomerID)
                  .OnDelete(DeleteBehavior.Restrict);

                // Enforce "only one feedback per (Order, Customer)"
                e.HasIndex(x => new { x.OrderID, x.CustomerID }).IsUnique();

                // Optional: rating 1–5 if present
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Feedback_RatingRange", "Rating IS NULL OR (Rating BETWEEN 1 AND 5)");
                });
            });

            // EMPLOYEE ASSIGNMENTS (join table)
            b.Entity<EmployeeAssignment>(e =>
            {
                e.HasKey(x => x.AssignmentID);
                e.Property(x => x.AssignedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Order)
                  .WithMany(o => o.EmployeeAssignments)
                  .HasForeignKey(x => x.OrderID)
                  .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Employee)
                  .WithMany(u => u.EmployeeAssignments)
                  .HasForeignKey(x => x.EmployeeID)
                  .OnDelete(DeleteBehavior.Restrict);

                // Prevent duplicate (Order, Employee) rows
                e.HasIndex(x => new { x.OrderID, x.EmployeeID }).IsUnique();
            });

            // AUDIT LOG
            b.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.LogID);
                e.Property(x => x.TableAffected).IsRequired().HasMaxLength(50);
                e.Property(x => x.Action).IsRequired().HasMaxLength(50);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(x => x.UserID)
                  .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // (Optional) centralize UpdatedAt stamping
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
              .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

            var now = DateTime.Now;

            foreach (var entry in entries)
            {
                switch (entry.Entity)
                {
                    case User u:
                        if (entry.State == EntityState.Added) u.CreatedAt = now;
                        u.UpdatedAt = now;
                        break;
                    case Order o:
                        if (entry.State == EntityState.Added) o.CreatedAt = now;
                        o.UpdatedAt = now;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}