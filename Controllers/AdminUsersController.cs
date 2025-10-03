using CISS411_GroupProject.Data;
using CISS411_GroupProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CISS411_GroupProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;

        public AdminUsersController(AppDbContext db,
                                    UserManager<IdentityUser> userMgr,
                                    RoleManager<IdentityRole> roleMgr)
        {
            _db = db;
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index()
        {
            var items = new List<UserRoleItem>();

            // load app users, include IdentityUserId & role snapshot
            var appUsers = await _db.AppUsers.AsNoTracking().OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();

            foreach (var u in appUsers)
            {
                var iUser = await _userMgr.FindByIdAsync(u.IdentityUserId);
                var roles = iUser is null ? new List<string>() : (await _userMgr.GetRolesAsync(iUser)).ToList();

                items.Add(new UserRoleItem
                {
                    AppUserId = u.UserID,
                    IdentityUserId = u.IdentityUserId,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    CurrentRole = roles.FirstOrDefault() ?? u.Role  // fallback to domain role
                });
            }

            ViewBag.AllRoles = (await _roleMgr.Roles.Select(r => r.Name!).ToListAsync());
            return View(items);
        }

        // POST: /AdminUsers/SetRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string identityUserId, int appUserId, string newRole)
        {
            // sanity checks
            if (string.IsNullOrWhiteSpace(newRole) || !await _roleMgr.RoleExistsAsync(newRole))
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction(nameof(Index));
            }

            var iUser = await _userMgr.FindByIdAsync(identityUserId);
            var appUser = await _db.AppUsers.FirstOrDefaultAsync(u => u.UserID == appUserId);

            if (iUser is null || appUser is null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Guard: don't remove the last Admin
            if (!string.Equals(newRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var isCurrentlyAdmin = await _userMgr.IsInRoleAsync(iUser, "Admin");
                if (isCurrentlyAdmin)
                {
                    var adminCount = await _db.AppUsers.CountAsync(u =>
                        _db.Users.Any() && // just to satisfy analyzer; not needed functionally
                        u.Role == "Admin"); // domain snapshot
                    // Better: count via Identity
                    var identityAdmins = await _userMgr.GetUsersInRoleAsync("Admin");
                    if (identityAdmins.Count <= 1)
                    {
                        TempData["Error"] = "You cannot demote the last Admin.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // Update Identity roles
            var currentRoles = await _userMgr.GetRolesAsync(iUser);
            if (currentRoles.Any())
                await _userMgr.RemoveFromRolesAsync(iUser, currentRoles);
            await _userMgr.AddToRoleAsync(iUser, newRole);

            // Sync domain role
            appUser.Role = newRole;
            appUser.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Message"] = "Role assigned successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
