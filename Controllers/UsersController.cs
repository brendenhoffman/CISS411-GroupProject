using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CISS411_GroupProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                user.Role = "Visitor";
                user.Status = "Pending Confirmation";

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Message"] = "Registration successful! Grandma Smith will contact you soon.";
                return RedirectToAction("Register");
            }

            return View(user);
        }

        // Optional: List page for Grandma Smith
        public IActionResult List()
        {
            var visitors = _context.Users.Where(u => u.Role == "Visitor").ToList();
            return View(visitors);
        }
    }
}
