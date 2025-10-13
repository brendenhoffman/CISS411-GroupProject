using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CISS411_GroupProject.Controllers
{
    [Authorize]
    public class DesignsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DesignsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ViewForOrder
        public async Task<IActionResult> ViewForOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            var currentUserId = GetLoggedInUserId();
            if (User.IsInRole("Customer") && order.CustomerID != currentUserId)
                return Forbid();

            // get the designs from database
            var designs = await _context.Designs
                .Where(d => d.OrderID == id)
                .Include(d => d.Employee)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            // convert to view model and get customer name if approved
            var designDetails = designs.Select(d => new DesignWithDetails
            {
                DesignID = d.DesignID,
                ImagePath = d.ImagePath,
                UploadedAt = d.UploadedAt,
                EmployeeName = d.Employee.FirstName + " " + d.Employee.LastName,
                ProposedQuantity = d.ProposedQuantity,
                EstimatedCost = d.EstimatedCost,
                DesignNotes = d.DesignNotes,
                IsApproved = d.IsApproved,
                ApprovedAt = d.ApprovedAt,
                ApprovedByCustomerID = d.ApprovedByCustomerID,
                ApprovedByCustomerName = d.ApprovedByCustomerID.HasValue
                    ? _context.AppUsers.Where(u => u.UserID == d.ApprovedByCustomerID.Value)
                        .Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                    : null
            }).ToList();

            var isCustomer = order.CustomerID == currentUserId;
            var isEmployee = User.IsInRole("Employee") || User.IsInRole("Admin");

            var viewModel = new OrderDesignsViewModel
            {
                Order = order,
                Designs = designDetails,
                CanApprove = isCustomer && order.Status == "Awaiting Customer Approval",
                CanUpload = isEmployee
            };

            return View(viewModel);
        }

        // Upload
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Upload(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            if (order.Status != "Pending")
            {
                TempData["Error"] = "Designs can only be uploaded for orders with 'Pending' status.";
                return RedirectToAction(nameof(ViewForOrder), new { id });
            }

            var itemsDesc = string.Join(", ", order.OrderItems.Select(i => $"{i.ItemName} (x{i.Quantity})"));

            var viewModel = new DesignProposalViewModel
            {
                OrderID = order.OrderID,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                Occasion = order.Occasion,
                CustomerBudget = order.Budget,
                DeliveryDate = order.DeliveryDate,
                OrderItemsDescription = itemsDesc
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Upload(DesignProposalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var orderForDisplay = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderID == model.OrderID);

                if (orderForDisplay != null)
                {
                    model.CustomerName = $"{orderForDisplay.Customer.FirstName} {orderForDisplay.Customer.LastName}";
                    model.Occasion = orderForDisplay.Occasion;
                    model.CustomerBudget = orderForDisplay.Budget;
                    model.DeliveryDate = orderForDisplay.DeliveryDate;
                    model.OrderItemsDescription = string.Join(", ",
                        orderForDisplay.OrderItems.Select(i => $"{i.ItemName} (x{i.Quantity})"));
                }
                return View(model);
            }

            var order = await _context.Orders.FindAsync(model.OrderID);
            if (order == null)
                return NotFound();

            if (order.Status != "Pending")
            {
                TempData["Error"] = "Designs can only be uploaded for orders with 'Pending' status.";
                return RedirectToAction(nameof(ViewForOrder), new { id = model.OrderID });
            }

            if (model.EstimatedCost > order.Budget)
            {
                ModelState.AddModelError(nameof(model.EstimatedCost),
                    $"Estimated cost cannot exceed customer budget of ${order.Budget:F2}");
                return View(model);
            }

            string? imagePath = null;
            if (model.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "designs");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/uploads/designs/{uniqueFileName}";
            }

            var design = new Design
            {
                OrderID = model.OrderID,
                EmployeeID = GetLoggedInUserId(),
                ImagePath = imagePath!,
                UploadedAt = DateTime.Now,
                ProposedQuantity = model.ProposedQuantity,
                EstimatedCost = model.EstimatedCost,
                DesignNotes = model.DesignNotes,
                IsApproved = false
            };

            _context.Designs.Add(design);

            order.Status = "Awaiting Customer Approval";
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserID = GetLoggedInUserId(),
                TableAffected = "Orders",
                RecordID = order.OrderID,
                Action = "DesignUploaded",
                OldValue = "Pending",
                NewValue = "Awaiting Customer Approval",
                CreatedAt = DateTime.Now
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Design uploaded successfully. Order status updated to 'Awaiting Customer Approval'.";
            return RedirectToAction(nameof(ViewForOrder), new { id = model.OrderID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int orderId)
        {
            var design = await _context.Designs
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
                return NotFound();

            var currentUserId = GetLoggedInUserId();

            if (design.Order.CustomerID != currentUserId)
            {
                TempData["Error"] = "Only the customer can approve designs.";
                return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
            }

            if (design.Order.Status != "Awaiting Customer Approval")
            {
                TempData["Error"] = "This order is not awaiting approval.";
                return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
            }

            design.IsApproved = true;
            design.ApprovedAt = DateTime.Now;
            design.ApprovedByCustomerID = currentUserId;

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderID == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                item.DesignApproved = true;
            }

            design.Order.Status = "In Process";
            design.Order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserID = currentUserId,
                TableAffected = "Orders",
                RecordID = orderId,
                Action = "DesignApproved",
                OldValue = "Awaiting Customer Approval",
                NewValue = "In Process",
                CreatedAt = DateTime.Now
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Design approved! Order status updated to 'In Process' on {DateTime.Now:g}.";
            return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, int orderId, string? rejectionReason)
        {
            var design = await _context.Designs
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
                return NotFound();

            var currentUserId = GetLoggedInUserId();

            if (design.Order.CustomerID != currentUserId)
            {
                TempData["Error"] = "Only the customer can reject designs.";
                return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
            }

            design.Order.Status = "Pending";
            design.Order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserID = currentUserId,
                TableAffected = "Orders",
                RecordID = orderId,
                Action = "DesignRejected",
                OldValue = "Awaiting Customer Approval",
                NewValue = "Pending",
                CreatedAt = DateTime.Now
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Design rejected. Order returned to 'Pending' status for new design submission.";
            return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Delete(int id, int orderId)
        {
            var design = await _context.Designs
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
                return NotFound();

            // Only allow deletion if design is not approved
            if (design.IsApproved)
            {
                TempData["Error"] = "Cannot delete an approved design.";
                return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
            }

            // Delete the image file from the server
            if (!string.IsNullOrEmpty(design.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, design.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with database deletion
                        System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                    }
                }
            }

            // Remove from database
            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();

            // Log the deletion
            var auditLog = new AuditLog
            {
                UserID = GetLoggedInUserId(),
                TableAffected = "Designs",
                RecordID = id,
                Action = "DesignDeleted",
                OldValue = $"Design #{id} for Order #{orderId}",
                NewValue = null,
                CreatedAt = DateTime.Now
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // If this was the last design and order is "Awaiting Customer Approval", 
            // return it to "Pending"
            var remainingDesigns = await _context.Designs
                .Where(d => d.OrderID == orderId)
                .CountAsync();

            if (remainingDesigns == 0 && design.Order.Status == "Awaiting Customer Approval")
            {
                design.Order.Status = "Pending";
                design.Order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var statusAuditLog = new AuditLog
                {
                    UserID = GetLoggedInUserId(),
                    TableAffected = "Orders",
                    RecordID = orderId,
                    Action = "StatusChanged",
                    OldValue = "Awaiting Customer Approval",
                    NewValue = "Pending",
                    CreatedAt = DateTime.Now
                };
                _context.AuditLogs.Add(statusAuditLog);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Design deleted successfully.";
            return RedirectToAction(nameof(ViewForOrder), new { id = orderId });
        }

        private int GetLoggedInUserId()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return 1;

            var user = _context.AppUsers.FirstOrDefault(u => u.Email == email);
            return user?.UserID ?? 1;
        }
    }
}