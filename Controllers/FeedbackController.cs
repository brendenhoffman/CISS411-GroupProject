//  Group 3 - Agile Minds: Linda Daniel, Jason Farr, Brenden Hoffman, Justin Kim, 
//	Allan Lopezsandoval, Ashley Steward, Juan Rodriguez, Jerome Whitaker
//  CISS 411 Software Architecture wASP.NET MVC
//  Team Project: Smith Sweet Shop
//  Linda: 10-13-25

using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CISS411_GroupProject.Controllers
{
	// Only customers can leave feedback
	[Authorize(Roles = "Customer")] 
	public class FeedbackController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public FeedbackController(AppDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET Feedback/Create
		public async Task<IActionResult> Create(int orderId)
		{
			// Get current user
			var identityUserId = _userManager.GetUserId(User);
			var appUser = await _context.AppUsers
				.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
			if (appUser == null)
				return Unauthorized();

			// Make sure order exists & belongs to this user
			var order = await _context.Orders
			// Include feedbacks to check for existing feedback
				.Include(o => o.Feedbacks)
				.FirstOrDefaultAsync(o => o.OrderID == orderId && o.CustomerID == appUser.UserID);
			if (order == null)
				return NotFound("Order not found.");

			// Check if feedback already exists for this order and user
			var existingFeedback = order.Feedbacks?
				.FirstOrDefault(f => f.CustomerID == appUser.UserID && !string.IsNullOrEmpty(f.FeedbackText));

			if (existingFeedback != null)
			{
				TempData["InfoMessage"] = "Feedback previously submitted. View in order details.";
				return RedirectToAction("Details", "Order", new { id = orderId });
			}

			// Pass current user ID to view for the feedback check
			ViewData["CurrentUserId"] = appUser.UserID;

			return View(new Feedback
			{
				OrderID = orderId,
				CustomerID = appUser.UserID,
				// Pass the order with included feedbacks to the view
				Order = order 
			});
		}

		// POST Feedback/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Feedback feedback)
		{
			var identityUserId = _userManager.GetUserId(User);
			var appUser = await _context.AppUsers
				.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

			if (appUser == null)
				return Unauthorized();

			// Check if feedback already exists before creating new one
			var existingFeedback = await _context.Feedbacks
				.FirstOrDefaultAsync(f => f.OrderID == feedback.OrderID &&
										f.CustomerID == appUser.UserID &&
										!string.IsNullOrEmpty(f.FeedbackText));

			if (existingFeedback != null)
			{
				TempData["InfoMessage"] = "Feedback previously submitted. View in order details.";
				return RedirectToAction("Details", "Order", new { id = feedback.OrderID });
			}

			if (!ModelState.IsValid)
			{
				feedback.CustomerID = appUser.UserID;
				return View(feedback);
			}

			feedback.CreatedAt = DateTime.Now;
			feedback.CustomerID = appUser.UserID;

			_context.Feedbacks.Add(feedback);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Thank you for your feedback. Grandma Smith will read it personally.";
			return RedirectToAction("Details", "Order", new { id = feedback.OrderID });
		}
	}
}


