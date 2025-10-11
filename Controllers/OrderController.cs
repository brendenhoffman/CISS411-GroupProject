﻿/*
Course #: CISS 411
Course Name: Software Architecture with ASP.NET with MVC
Group 3: Ashley Steward, Linda Daniel, Allan Lopesandovall,
Brenden Hoffman, Jason Farr, Jerome Whitaker,
Jason Farr and Justin Kim.
Date Completed: 10-2-2025
Story Assignee: Ashley Steward
Story: User Story 2
*/

using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CISS411_GroupProject.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Order/Create
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create()
        {
            var availableItems = await GetAvailableItemsAsync();

            var model = new OrderFormViewModel
            {
                OrderInput = new Order { DeliveryDate = DateTime.Today.AddDays(1) },
                Items = new List<OrderItem>(), // start empty; the view lets user add rows
                AvailableItems = availableItems
            };

            return View(model);
        }

        // POST: /Order/Create
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderFormViewModel model)
        {
            foreach (var kvp in ModelState)
            {
                var errors = kvp.Value?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    Console.WriteLine($"MODELSTATE: {kvp.Key} => {string.Join("; ", errors.Select(e => e.ErrorMessage))}");
                }
            }
            // Ensure collections exist when re-rendering the view
            model.Items ??= new List<OrderItem>();

            // These aren’t posted from the form; we’ll set them server-side
            ModelState.Remove("OrderInput");             // top-level complex property not posted as a single value
            ModelState.Remove("OrderInput.Customer");    // navigation prop
            ModelState.Remove("OrderInput.CustomerID");  // FK
            ModelState.Remove("OrderInput.OrderID");     // identity key, not posted

            // Bind logged-in user to the order BEFORE validation
            var userId = await GetLoggedInUserIdAsync();
            if (userId == 0)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
            }
            else
            {
                model.OrderInput.CustomerID = userId;
            }

            // Trim away blank items
            model.Items = model.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemName) && i.Quantity > 0)
                .ToList();

            // Per-item validation
            for (int i = 0; i < model.Items.Count; i++)
            {
                var item = model.Items[i];

                if (item.ItemName == "Custom Design" && string.IsNullOrWhiteSpace(item.CustomDescription))
                {
                    ModelState.AddModelError($"Items[{i}].CustomDescription", "Custom description is required for custom items.");
                }

                if (item.ItemName != "Custom Design")
                {
                    item.CustomDescription = null; // don’t carry a desc for standard items
                }
            }

            // Require at least one item
            if (!model.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item to your order.");
            }

            foreach (var kvp in ModelState.Where(k => k.Value?.Errors.Count > 0))
            {
                Console.WriteLine($"{kvp.Key} => {string.Join("; ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}");
            }

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                model.AvailableItems = await GetAvailableItemsAsync();
                return View(model);
            }

            try
            {
                model.OrderInput.Status = "Pending";
                model.OrderInput.CreatedAt = DateTime.Now;

                _context.Orders.Add(model.OrderInput);
                await _context.SaveChangesAsync(); // get OrderID

                foreach (var item in model.Items)
                {
                    item.OrderID = model.OrderInput.OrderID;

                    if (item.ItemName == "Custom Design")
                    {
                        item.DesignApproved = false; // requires approval
                    }
                    else
                    {
                        item.DesignApproved = true;  // standard items auto-approved
                        item.CustomDescription = null;
                    }

                    _context.OrderItems.Add(item);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your order has been placed successfully!";
                return RedirectToAction(nameof(Details), new { id = model.OrderInput.OrderID });
            }
            catch (Exception ex)
            {
                // You can log ex here
                ModelState.AddModelError(string.Empty, "An error occurred while processing your order. Please try again.");
                model.AvailableItems = await GetAvailableItemsAsync();
                return View(model);
            }
        }

        // GET: /Order/Details/{id}
        [Authorize(Roles = "Admin,Employee,Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            // Customers can only view their own orders
            if (User.IsInRole("Customer"))
            {
                var userId = await GetLoggedInUserIdAsync();
                if (order.CustomerID != userId) return Forbid();
            }

            return View(order);
        }

        // GET: /Order/List
        [Authorize(Roles = "Admin,Employee,Customer")]
        public async Task<IActionResult> List(string? status = null)
        {
            var q = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (User.IsInRole("Customer"))
            {
                var userId = await GetLoggedInUserIdAsync();
                q = q.Where(o => o.CustomerID == userId);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                q = q.Where(o => o.Status == status);
            }

            var orders = await q
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // POST: /Order/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
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
            return await _context.OrderItems
                .Select(oi => oi.ItemName)
                .Distinct()
                .Where(name => name != "Custom Design")
                .OrderBy(name => name)
                .Select(name => new SelectListItem { Value = name, Text = name })
                .ToListAsync();
        }

        // Helper: Get logged-in AppUser ID from Identity
        private async Task<int> GetLoggedInUserIdAsync()
        {
            var identityUserId = _userManager.GetUserId(User);
            if (identityUserId == null) return 0;

            var appUser = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            return appUser?.UserID ?? 0;
        }
        [HttpPost]
        public async Task<IActionResult> ApproveDesign(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.Status = "Approved";
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Design approved successfully!";
            return RedirectToAction(nameof(Details), new { id = order.OrderID });
        }
    }
}
