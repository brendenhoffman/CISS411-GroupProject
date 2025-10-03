using CISS411_GroupProject.Data;
using CISS411_GroupProject.Models;
using CISS411_GroupProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CISS411_GroupProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UsersController(AppDbContext context,
                               UserManager<IdentityUser> userManager,
                               SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Users/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Users/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // 1) Block duplicate email via Identity
            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already registered.");
                return View(vm);
            }

            // 2) Create Identity user (stores email + hashed password)
            var iUser = new IdentityUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                EmailConfirmed = true
            };

            var create = await _userManager.CreateAsync(iUser, vm.Password);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            // 3) Add Identity role 'Visitor'
            await _userManager.AddToRoleAsync(iUser, "Visitor");

            // 4) Create domain AppUsers row linked to Identity
            var appUser = new User
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Address = vm.Address,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                Role = "Visitor",
                Status = "Pending Confirmation",
                IdentityUserId = iUser.Id,
                CreatedAt = DateTime.Now
            };

            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync();

            // 5) Sign in
            await _signInManager.SignInAsync(iUser, isPersistent: false);

            TempData["Message"] = "Registration successful!";
            return RedirectToAction(nameof(Register));
        }

        // List page for Grandma Smith (admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> List()
        {
            var visitors = await _context.AppUsers
                .Where(u => u.Role == "Visitor")
                .AsNoTracking()
                .ToListAsync();

            return View(visitors);
        }
    }
}
