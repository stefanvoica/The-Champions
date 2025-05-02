using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string Email, string UserName, string UserRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["message"] = "Utilizatorul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            user.Email = Email;
            user.UserName = UserName;

            // Actualizare rol
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeRolesResult.Succeeded)
            {
                TempData["message"] = "Eroare la eliminarea rolurilor existente.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, UserRole);

            if (!addRoleResult.Succeeded)
            {
                TempData["message"] = "Eroare la actualizarea rolului.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            TempData["message"] = "Utilizatorul a fost actualizat cu succes.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["message"] = "Utilizatorul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeRolesResult.Succeeded)
            {
                TempData["message"] = "Eroare la eliminarea rolurilor existente.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, newRole);

            if (!addRoleResult.Succeeded)
            {
                TempData["message"] = "Eroare la adăugarea noului rol.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            TempData["message"] = "Rolul utilizatorului a fost actualizat cu succes.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RevokeUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["message"] = "Utilizatorul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (result.Succeeded)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                await _userManager.UpdateAsync(user);

                TempData["message"] = "Utilizatorul a fost blocat și toate rolurile i-au fost revocate.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "A apărut o eroare la revocarea rolurilor.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }

    }
}
