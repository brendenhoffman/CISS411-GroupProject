/*
Course #: CISS 411
Course Name: Software Architecture with ASP.NET with MVC
Group 3: Ashley Steward, Linda Daniel,Allan Lopesandovall,
Brenden Hoffman, Jason Farr, Jerome Whitaker,
Jason Farr and Justin Kim.
Date Completed: 10-2-2025
Story Assigne: Ashley Steward 
Story: User Story 2
*/


using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrderController : Controller
{
	private readonly AppDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;

	public OrderController(AppDbContext context, UserManager<IdentityUser> userManager)
	{
		_context = context;
		_userManager = userManager;
	}

	// GET: Order/Create
	public async Task<IActionResult> Create()
	{
		// Get available items from existing OrderItems
		var availableItems = await _context.OrderItems
			.Select(oi => oi.ItemName)
			.Distinct()
			.Where(name => name != "Custom Design") // Exclude custom design from standard list
			.OrderBy(name => name)
			.Select(name => new SelectListItem { Value = name, Text = name })
			.ToListAsync();

		var model = new OrderFormViewModel
		{
			Order = new Order { DeliveryDate = DateTime.Today.AddDays(1) },
			Items = new List<OrderItem>(), // Start with EMPTY list
			AvailableItems = availableItems
		};

		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(OrderFormViewModel model)
	{
		// DEBUG: Check what's coming in
		Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
		Console.WriteLine($"Items count: {model.Items?.Count}");

		if (model.Items != null)
		{
			for (int i = 0; i < model.Items.Count; i++)
			{
				var item = model.Items[i];
				Console.WriteLine($"Item {i}: Name='{item.ItemName}', Quantity={item.Quantity}, CustomDesc='{item.CustomDescription}'");
			}

			// Remove empty items
			model.Items = model.Items
				.Where(item => !string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0)
				.ToList();
		}



		// Custom validation for items
		if (model.Items != null)
		{
			for (int i = 0; i < model.Items.Count; i++)
			{
				var item = model.Items[i];

				// Custom items require description
				if (item.ItemName == "Custom Design" && string.IsNullOrWhiteSpace(item.CustomDescription))
				{
					ModelState.AddModelError($"Items[{i}].CustomDescription", "Custom description is required for custom items.");
				}

				// Standard items should not have custom description
				if (item.ItemName != "Custom Design")
				{
					item.CustomDescription = null;
				}
			}
		}



		// Check for valid items
		if (model.Items == null || !model.Items.Any())
		{
			ModelState.AddModelError("", "Please add at least one item to your order.");
		}

		// Check ModelState after our custom validation
		if (!ModelState.IsValid)
		{
			Console.WriteLine("ModelState is invalid. Errors:");
			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				Console.WriteLine($" - {error.ErrorMessage}");
			}

			model.AvailableItems = await GetAvailableItemsAsync();
			return View(model);
		}

		try
		{
			var userId = await GetLoggedInUserIdAsync();
			if (userId == 0)
			{
				ModelState.AddModelError("", "User not found.");
				model.AvailableItems = await GetAvailableItemsAsync();
				return View(model);
			}

			model.Order.CustomerID = userId;
			model.Order.Status = "Pending";
			model.Order.CreatedAt = DateTime.Now;

			_context.Orders.Add(model.Order);
			await _context.SaveChangesAsync(); // Save to get OrderID

			// Process order items
			foreach (var item in model.Items)
			{
				if (!string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0)
				{
					item.OrderID = model.Order.OrderID;

					// Handle custom design
					if (item.ItemName == "Custom Design")
					{
						item.DesignApproved = false; // needs approval for custom items
													 // CustomDescription should already be set from the form
					}
					else
					{
						item.DesignApproved = true; // standard items auto-approved
						item.CustomDescription = null; // Clear for standard items
					}

					_context.OrderItems.Add(item);
				}
			}

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Your order has been placed successfully!";
			return RedirectToAction(nameof(Details), new { id = model.Order.OrderID });
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception: {ex.Message}");
			Console.WriteLine($"Stack Trace: {ex.StackTrace}");

			ModelState.AddModelError("", "An error occurred while processing your order. Please try again or call 1-800-555-SMITH.");
			model.AvailableItems = await GetAvailableItemsAsync();
			return View(model);
		}
	}

	/*  LINDA: REMOVED TO UPDATE CREATE()
	// POST: Order/Create
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(OrderFormViewModel model)
	{
		// Remove empty items before validation
		if (model.Items != null)
		{
			model.Items = model.Items
				.Where(item => !string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0)
				.ToList();
		}

		if (!ModelState.IsValid || model.Items == null || !model.Items.Any())
		{
			if (!model.Items.Any())
			{
				ModelState.AddModelError("", "Please add at least one item to your order.");
			}

			// Repopulate AvailableItems
			model.AvailableItems = await GetAvailableItemsAsync();
			return View(model);
		}

		var userId = await GetLoggedInUserIdAsync();
		if (userId == 0)
		{
			ModelState.AddModelError("", "User not found.");
			model.AvailableItems = await GetAvailableItemsAsync();
			return View(model);
		}

		model.Order.CustomerID = userId;
		model.Order.Status = "Pending";
		model.Order.CreatedAt = DateTime.Now;

		_context.Orders.Add(model.Order);
		await _context.SaveChangesAsync();

		// Process order items
		foreach (var item in model.Items)
		{
			if (!string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0)
			{
				item.OrderID = model.Order.OrderID;

				// Handle custom design
				if (item.ItemName == "Custom Design" && !string.IsNullOrWhiteSpace(item.CustomDescription))
				{
					// Keep "Custom Design" as ItemName, store details in CustomDescription
					item.DesignApproved = false; // needs approval for custom items
				}
				else
				{
					item.DesignApproved = true; // standard items auto-approved
					item.CustomDescription = null; // Clear for standard items
				}

				_context.OrderItems.Add(item);
			}
		}

		await _context.SaveChangesAsync();

		TempData["SuccessMessage"] = "Your order has been placed successfully!";
		return RedirectToAction(nameof(Details), new { id = model.Order.OrderID });
	}
	*/


	// GET: Order/Details/{id}
	public async Task<IActionResult> Details(int id)
	{
		var order = await _context.Orders
			.Include(o => o.Customer)
			.Include(o => o.OrderItems)
			.FirstOrDefaultAsync(o => o.OrderID == id);

		if (order == null) return NotFound();
		return View(order);
	}

	// GET: Order/List
	public async Task<IActionResult> List(string status = null)
	{
		var orders = _context.Orders
			.Include(o => o.Customer)
			.Include(o => o.OrderItems)
			.AsQueryable();

		if (User.IsInRole("Customer"))
		{
			var userId = await GetLoggedInUserIdAsync();
			orders = orders.Where(o => o.CustomerID == userId);
		}
		else if (!string.IsNullOrEmpty(status))
		{
			orders = orders.Where(o => o.Status == status);
		}

		return View(await orders.ToListAsync());
	}

	// POST: Order/UpdateStatus
	[HttpPost]
	public async Task<IActionResult> UpdateStatus(int id, string status)
	{
		var order = await _context.Orders.FindAsync(id);
		if (order == null) return NotFound();

		order.Status = status;
		order.UpdatedAt = DateTime.Now;
		_context.Update(order);
		await _context.SaveChangesAsync();

		TempData["SuccessMessage"] = $"Order status updated to {status}.";
		return RedirectToAction(nameof(Details), new { id });
	}

	// Helper: Get available items from existing OrderItems
	private async Task<List<SelectListItem>> GetAvailableItemsAsync()
	{
		var items = await _context.OrderItems
			.Select(oi => oi.ItemName)
			.Distinct()
			.Where(name => name != "Custom Design") // Exclude custom design from standard list
			.OrderBy(name => name)
			.Select(name => new SelectListItem { Value = name, Text = name })
			.ToListAsync();
		return items;
	}

	// Helper: Get logged-in user ID from Identity
	private async Task<int> GetLoggedInUserIdAsync()
	{
		var identityUserId = _userManager.GetUserId(User);
		var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
		return appUser?.UserID ?? 0;
	}
}
