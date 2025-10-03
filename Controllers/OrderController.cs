using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
/*Course #: CISS 411
Course Name: Software Architecture with ASP.NET with MVC
Group 2: Ashley Steward, Linda Daniel,Allan Lopesandovall,
Brenden Hoffman, Jason Farr, Jerome Whitaker,
Jason Farr and Justin Kim.
Date Completed: 10-2-2025
Story Assigne: Ashley Steward 
Story: User Story 2
*/

public class OrderController : Controller
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    // To request the view contents
    public IActionResult Create()
    {
        var model = new OrderFormViewModel
        {
            Order = new Order()
            {
                DeliveryDate = DateTime.Today.AddDays(1) // default tomorrow
            },
            Items = new List<OrderItem> { new OrderItem() }
        };
        return View(model);
    }

    // To post view contents
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = GetLoggedInUserId();
            model.Order.CustomerID = userId;
            model.Order.Status = "Pending";
            model.Order.CreatedAt = DateTime.Now;

            _context.Orders.Add(model.Order);
            await _context.SaveChangesAsync();

            foreach (var item in model.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0)
                {
                    item.OrderID = model.Order.OrderID;
                    _context.OrderItems.Add(item);
                }
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your order has been placed successfully!";

            return RedirectToAction(nameof(Details), new { id = model.Order.OrderID });
        }

        return View(model);
    }

    // To get order details 
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderID == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // To get the list of products for Grandma and employees
    public async Task<IActionResult> List(string status = null)
    {
        var orders = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            orders = orders.Where(o => o.Status == status);
        }

        return View(await orders.ToListAsync());
    }

    // To update the status of the products
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();

        }

        order.Status = status;
        order.UpdatedAt = DateTime.Now;
        _context.Update(order);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Order status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private int GetLoggedInUserId()
    {
        return 1; // temporary authentication. Replace when you wire up Identity/Auth
    }
}
