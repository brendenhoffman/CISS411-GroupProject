using CISS411_GroupProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CISS411_GroupProject.Data
{
	public static class DbInitializer
	{
		public static void Initialize(AppDbContext context)
		{
			// Ensure database creation. Apply migrations if needed
			context.Database.Migrate();

			// Seed Users
			if (!context.Users.Any(u => u.Email == "grandma@smithsweets.com"))
			{
				var users = new User[]
				{
					new User { FirstName = "Grandma", LastName = "Smith", Address = "123 Main St", Email = "grandma@smithsweets.com", PhoneNumber = "555-0001", Role = "Admin", Status = "Active" },

                    // Two active customers
                    new User { FirstName = "Alice", LastName = "Johnson", Address = "456 Oak Ave", Email = "alice@test.com", PhoneNumber = "555-1001", Role = "Customer", Status = "Active" },
					new User { FirstName = "Bob", LastName = "Williams", Address = "789 Pine Rd", Email = "bob@test.com", PhoneNumber = "555-1002", Role = "Customer", Status = "Active" },

                    // Three employees
                    new User { FirstName = "Carol", LastName = "Davis", Address = "321 Elm St", Email = "carol@test.com", PhoneNumber = "555-2001", Role = "Employee", Status = "Active" },
					new User { FirstName = "David", LastName = "Miller", Address = "654 Maple Ln", Email = "david@test.com", PhoneNumber = "555-2002", Role = "Employee", Status = "Active" },
					new User { FirstName = "Eve", LastName = "Brown", Address = "987 Cedar Blvd", Email = "eve@test.com", PhoneNumber = "555-2003", Role = "Employee", Status = "Active" },

                    // Three visitors (pending confirmation)
                    new User { FirstName = "Frank", LastName = "Wilson", Address = "111 Birch St", Email = "frank@test.com", PhoneNumber = "555-3001", Role = "Visitor", Status = "Pending Confirmation" },
					new User { FirstName = "Grace", LastName = "Moore", Address = "222 Spruce St", Email = "grace@test.com", PhoneNumber = "555-3002", Role = "Visitor", Status = "Pending Confirmation" },
					new User { FirstName = "Hank", LastName = "Taylor", Address = "333 Walnut St", Email = "hank@test.com", PhoneNumber = "555-3003", Role = "Visitor", Status = "Pending Confirmation" }
				};

				context.Users.AddRange(users);
				context.SaveChanges();
			}

			// Seed Orders to active customers
			if (!context.Orders.Any())
			{
				var alice = context.Users.First(u => u.Email == "alice@test.com");
				var bob = context.Users.First(u => u.Email == "bob@test.com");

				var orders = new Order[]
				{
					new Order { CustomerID = alice.UserID, Occasion = "Birthday", DeliveryDate = DateTime.Today.AddDays(7), Budget = 50m, Status = "Pending" },
					new Order { CustomerID = alice.UserID, Occasion = "Anniversary", DeliveryDate = DateTime.Today.AddDays(14), Budget = 75m, Status = "Pending" },
					new Order { CustomerID = bob.UserID, Occasion = "Graduation", DeliveryDate = DateTime.Today.AddDays(10), Budget = 100m, Status = "Pending" }
				};

				context.Orders.AddRange(orders);
				context.SaveChanges();
			}

			// ORDER ITEMS
			if (!context.OrderItems.Any())
			{
				var orders = context.Orders.ToList();
				if (orders.Count >= 3)
				{
					var orderItems = new OrderItem[]
					{
						new OrderItem { OrderID = orders[0].OrderID, ItemName = "Chocolate Cake", Quantity = 1, DesignApproved = false },
						new OrderItem { OrderID = orders[1].OrderID, ItemName = "Cupcake Box", Quantity = 12, DesignApproved = false },
						new OrderItem { OrderID = orders[2].OrderID, ItemName = "Graduation Cookies", Quantity = 24, DesignApproved = true }
					};

					context.OrderItems.AddRange(orderItems);
					context.SaveChanges();
				}
			}

			// EMPLOYEE ASSIGNMENTS
			if (!context.EmployeeAssignments.Any())
			{
				var orders = context.Orders.ToList();
				var employees = context.Users.Where(u => u.Role == "Employee").ToList();

				if (orders.Any() && employees.Count >= 2)
				{
					var assignments = new EmployeeAssignment[]
					{
						new EmployeeAssignment { OrderID = orders[0].OrderID, EmployeeID = employees[0].UserID, AssignedAt = DateTime.Now },
						new EmployeeAssignment { OrderID = orders[1].OrderID, EmployeeID = employees[1].UserID, AssignedAt = DateTime.Now }
					};

					context.EmployeeAssignments.AddRange(assignments);
					context.SaveChanges();
				}
			}

			// DESIGNS
			if (!context.Designs.Any())
			{
				var orders = context.Orders.ToList();
				var employees = context.Users.Where(u => u.Role == "Employee").ToList();

				if (orders.Any() && employees.Any())
				{
					var designs = new Design[]
					{
						new Design { OrderID = orders[0].OrderID, EmployeeID = employees[0].UserID, ImagePath = "/images/birthday_cake.png", UploadedAt = DateTime.Now },
						new Design { OrderID = orders[1].OrderID, EmployeeID = employees[1].UserID, ImagePath = "/images/cupcake_box.png", UploadedAt = DateTime.Now }
					};

					context.Designs.AddRange(designs);
					context.SaveChanges();
				}
			}

			// FEEDBACK – add only if at least one order is "Picked Up"
			if (!context.Feedbacks.Any())
			{
				var completedOrder = context.Orders.FirstOrDefault(o => o.Status == "Picked Up");
				if (completedOrder != null)
				{
					var feedback = new Feedback
					{
						OrderID = completedOrder.OrderID,
						CustomerID = completedOrder.CustomerID,
						Rating = 5,
						FeedbackText = "Everything was perfect, thank you!",
						CreatedAt = DateTime.Now
					};

					context.Feedbacks.Add(feedback);
					context.SaveChanges();
				}
			}
		}
	}
}
