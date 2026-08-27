using Inventory.DataAccess.Data;
using Inventory.Models;
using Inventory.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    [Area("User")]
    public class RoleSwitchController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleSwitchController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Switch(string targetRole, string? returnUrl = null)
        {
            string mappedRole = targetRole switch
            {
                "Admin" => SD.Role_Admin,
                "Supplier" => SD.Role_Supplier,
                "SupplyHandler" => SD.Role_SupplyHandler,
                _ => SD.Role_Admin
            };

            string defaultEmail = targetRole switch
            {
                "Admin" => "admin@mponline.com",
                "Supplier" => "supplier@mponline.com",
                "SupplyHandler" => "handler@mponline.com",
                _ => "admin@mponline.com"
            };

            // Ensure role exists in RoleManager
            if (!await _roleManager.RoleExistsAsync(mappedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(mappedRole));
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    // Remove all current view roles
                    var currentRoles = await _userManager.GetRolesAsync(currentUser);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(currentUser, currentRoles);
                    }

                    // Add new role
                    await _userManager.AddToRoleAsync(currentUser, mappedRole);

                    // Refresh sign in so user claims update immediately
                    await _signInManager.RefreshSignInAsync(currentUser);

                    TempData["success"] = $"Switched view to {targetRole} successfully!";
                }
            }
            else
            {
                // Auto login as default user for this role
                var defaultUser = await _userManager.FindByEmailAsync(defaultEmail);
                if (defaultUser != null)
                {
                    await _signInManager.SignInAsync(defaultUser, isPersistent: true);
                    TempData["success"] = $"Signed in as {targetRole} view!";
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return targetRole switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                "Supplier" => RedirectToAction("Index", "Suppliers", new { area = "Admin" }),
                "SupplyHandler" => RedirectToAction("Index", "StockAdjustment", new { area = "Admin" }),
                _ => RedirectToAction("Index", "Home", new { area = "User" })
            };
        }
    }
}
