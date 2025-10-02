using Microsoft.AspNetCore.Mvc;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.Data;
using System;
using System.Linq;

namespace CISS411_GroupProject.Models
{
    public class NotificationsController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult NewRegistrations()
        {
            {
                // Get users registered in last 24 hours
                var newUsers = _context.Users
                    .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-1) && u.Role =="Visitor" && u.Status == "Pending Confirmation")
                    .Select(u => new { u.FirstName, u.LastName, u.Email })
                    .ToList();

                return Json(newUsers);
            }
        }
    }
}
