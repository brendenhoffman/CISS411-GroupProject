using CISS411_GroupProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CISS411_GroupProject.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 0) Ensure DB/migrations
            await context.Database.MigrateAsync();

            // 1) Roles
            string[] roles = { "Admin", "Employee", "Customer", "Visitor" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // 2) Users (helper creates BOTH Identity + AppUsers)
            async Task<User> EnsureUserAsync(
                string email, string password, string role,
                string first, string last, string address, string phone, string status = "Active")
            {
                // Identity user
                var iUser = await userMgr.FindByEmailAsync(email);
                if (iUser is null)
                {
                    iUser = new IdentityUser { UserName = email, Email = email, PhoneNumber = phone, EmailConfirmed = true };
                    var result = await userMgr.CreateAsync(iUser, password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new Exception($"Failed to create Identity user {email}: {errors}");
                    }
                }
                // Role
                if (!await userMgr.IsInRoleAsync(iUser, role))
                    await userMgr.AddToRoleAsync(iUser, role);

                // Domain user (AppUsers) linked via IdentityUserId
                var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
                if (appUser is null)
                {
                    appUser = new User
                    {
                        FirstName = first,
                        LastName = last,
                        Address = address,
                        Email = email,
                        PhoneNumber = phone,
                        Role = role,
                        Status = status,
                        IdentityUserId = iUser.Id,
                        CreatedAt = DateTime.Now
                    };
                    context.AppUsers.Add(appUser);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // keep role/status in sync if needed
                    if (appUser.Role != role || appUser.IdentityUserId != iUser.Id)
                    {
                        appUser.Role = role;
                        appUser.IdentityUserId = iUser.Id;
                        appUser.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                }
                return appUser;
            }

            // 2a) Seed people (choose simple dev passwords)
            // You can change these or move to appsettings.Development.json if you prefer
            var grandma = await EnsureUserAsync("grandma@smithsweets.com", "Passw0rd!", "Admin",
                "Grandma", "Smith", "123 Main St", "555-0001");

            var alice = await EnsureUserAsync("alice@test.com", "Passw0rd!", "Customer",
                "Alice", "Johnson", "456 Oak Ave", "555-1001");

            var bob = await EnsureUserAsync("bob@test.com", "Passw0rd!", "Customer",
                "Bob", "Williams", "789 Pine Rd", "555-1002");

            var carol = await EnsureUserAsync("carol@test.com", "Passw0rd!", "Employee",
                "Carol", "Davis", "321 Elm St", "555-2001");

            var david = await EnsureUserAsync("david@test.com", "Passw0rd!", "Employee",
                "David", "Miller", "654 Maple Ln", "555-2002");

            var eve = await EnsureUserAsync("eve@test.com", "Passw0rd!", "Employee",
                "Eve", "Brown", "987 Cedar Blvd", "555-2003");

            var frank = await EnsureUserAsync("frank@test.com", "Passw0rd!", "Visitor",
                "Frank", "Wilson", "111 Birch St", "555-3001", status: "Pending Confirmation");

            var grace = await EnsureUserAsync("grace@test.com", "Passw0rd!", "Visitor",
                "Grace", "Moore", "222 Spruce St", "555-3002", status: "Pending Confirmation");

            var hank = await EnsureUserAsync("hank@test.com", "Passw0rd!", "Visitor",
                "Hank", "Taylor", "333 Walnut St", "555-3003", status: "Pending Confirmation");

            // 3) Seed orders (using AppUsers)
            if (!await context.Orders.AnyAsync())
            {
                var orders = new[]
                {
                    new Order { CustomerID = alice.UserID, Occasion = "Birthday",    DeliveryDate = DateTime.Today.AddDays(7),  Budget = 50m,  Status = "Pending" },
                    new Order { CustomerID = alice.UserID, Occasion = "Anniversary", DeliveryDate = DateTime.Today.AddDays(14), Budget = 75m,  Status = "Pending" },
                    new Order { CustomerID = bob.UserID,   Occasion = "Graduation",  DeliveryDate = DateTime.Today.AddDays(10), Budget = 100m, Status = "Pending" }
                };
                context.Orders.AddRange(orders);
                await context.SaveChangesAsync();
            }

            // 4) Seed order items
            if (!await context.OrderItems.AnyAsync())
            {
                var orders = await context.Orders.OrderBy(o => o.OrderID).ToListAsync();
                if (orders.Count >= 3)
                {
                    var items = new[]
                    {
                        new OrderItem { OrderID = orders[0].OrderID, ItemName = "Chocolate Cake",  Quantity = 1,  DesignApproved = false },
                        new OrderItem { OrderID = orders[1].OrderID, ItemName = "Cupcake Box",     Quantity = 12, DesignApproved = false },
                        new OrderItem { OrderID = orders[2].OrderID, ItemName = "Iced Sugar Cookies", Quantity = 24, DesignApproved = true }
                    };
                    context.OrderItems.AddRange(items);
                    await context.SaveChangesAsync();
                }
            }

            // 5) Seed employee assignments
            if (!await context.EmployeeAssignments.AnyAsync())
            {
                var orders = await context.Orders.OrderBy(o => o.OrderID).ToListAsync();
                var employees = await context.AppUsers.Where(u => u.Role == "Employee").ToListAsync();

                if (orders.Any() && employees.Count >= 2)
                {
                    var assignments = new[]
                    {
                        new EmployeeAssignment { OrderID = orders[0].OrderID, EmployeeID = employees[0].UserID, AssignedAt = DateTime.Now },
                        new EmployeeAssignment { OrderID = orders[1].OrderID, EmployeeID = employees[1].UserID, AssignedAt = DateTime.Now }
                    };
                    context.EmployeeAssignments.AddRange(assignments);
                    await context.SaveChangesAsync();
                }
            }

            // 6) Seed designs
            if (!await context.Designs.AnyAsync())
            {
                var orders = await context.Orders.OrderBy(o => o.OrderID).ToListAsync();
                var employees = await context.AppUsers.Where(u => u.Role == "Employee").ToListAsync();

                if (orders.Any() && employees.Any())
                {
                    var designs = new[]
                    {
                        new Design { OrderID = orders[0].OrderID, EmployeeID = employees[0].UserID, ImagePath = "/images/birthday_cake.png", UploadedAt = DateTime.Now },
                        new Design { OrderID = orders[1].OrderID, EmployeeID = employees[1].UserID, ImagePath = "/images/cupcake_box.png",  UploadedAt = DateTime.Now }
                    };
                    context.Designs.AddRange(designs);
                    await context.SaveChangesAsync();
                }
            }

            // feedback seeding left as-is (only when there's a Picked Up order) — optional
        }
    }
}
