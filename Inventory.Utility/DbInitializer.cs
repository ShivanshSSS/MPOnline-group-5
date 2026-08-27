using Inventory.DataAccess.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Utility
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            // Ensure SQLite database is created
            try
            {
                _db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }

            // Create roles if they don't exist
            try
            {
                // Ensure all roles exist
                string[] roles = new[] { SD.Role_Admin, SD.Role_Supplier, SD.Role_SupplyHandler, SD.Role_Manager, SD.Role_Employee, SD.Role_Viewer };
                foreach (var role in roles)
                {
                    if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }

                // Helper local function to seed a user if missing
                void EnsureUserCreated(string email, string name, string password, string roleName)
                {
                    var existingUser = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
                    if (existingUser == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            Name = name,
                            PhoneNumber = "1234567890",
                            StreetAddress = "123 MP Online St",
                            State = "IL",
                            PostalCode = "60601",
                            City = "Chicago",
                            EmailConfirmed = true
                        };
                        var createRes = _userManager.CreateAsync(newUser, password).GetAwaiter().GetResult();
                        if (createRes.Succeeded)
                        {
                            _userManager.AddToRoleAsync(newUser, roleName).GetAwaiter().GetResult();
                            Console.WriteLine($"Created account {email} with role {roleName}");
                        }
                    }
                }

                EnsureUserCreated("admin@inventory.com", "System Administrator", "Admin@123", SD.Role_Admin);
                EnsureUserCreated("admin@mponline.com", "Admin Group 5", "Admin@123", SD.Role_Admin);
                EnsureUserCreated("supplier@mponline.com", "Supplier Representative", "Supplier@123", SD.Role_Supplier);
                EnsureUserCreated("handler@mponline.com", "Supply Handler Specialist", "Handler@123", SD.Role_SupplyHandler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return;
        }
    }
}