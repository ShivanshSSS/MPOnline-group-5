using Inventory.DataAccess.Data;
using Inventory.Models;
using Inventory.Models.Models;
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
            try
            {
                _db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }

            try
            {
                // Ensure Roles
                string[] roles = new[] { SD.Role_Admin, "Debug", SD.Role_Supplier, SD.Role_SupplyHandler, SD.Role_Manager, SD.Role_Employee };
                foreach (var role in roles)
                {
                    if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }

                // Seed Users
                void EnsureUser(string email, string name, string password, string role)
                {
                    var user = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
                    if (user == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            Name = name,
                            PhoneNumber = "9876543210",
                            StreetAddress = "MP Online Dark Store Hub #4",
                            City = "Bengaluru",
                            State = "KA",
                            PostalCode = "560038",
                            EmailConfirmed = true
                        };
                        var res = _userManager.CreateAsync(newUser, password).GetAwaiter().GetResult();
                        if (res.Succeeded)
                        {
                            _userManager.AddToRoleAsync(newUser, role).GetAwaiter().GetResult();
                        }
                    }
                }

                EnsureUser("admin@mponline.com", "MP Online Warehouse Admin", "Admin123!", SD.Role_Admin);
                EnsureUser("debug@mponline.com", "MP Online Debug Engineer", "Debug123!", "Debug");

                // Seed Dark Stores
                if (!_db.DarkStores.Any())
                {
                    _db.DarkStores.AddRange(
                        new DarkStore { StoreName = "MP Online Dark Store - Indiranagar Hub", City = "Bengaluru", Address = "100 Feet Rd, Indiranagar", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Koramangala Hub", City = "Bengaluru", Address = "80 Feet Rd, Koramangala 4th Block", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Cyber City Hub", City = "Gurugram", Address = "DLF Phase 2, Cyber City", IsActive = true }
                    );
                    _db.SaveChanges();
                }

                // Seed Suppliers
                if (!_db.Suppliers.Any())
                {
                    _db.Suppliers.Add(new Supplier
                    {
                        SupplierName = "MP Online National Procurement Ltd",
                        ContactPerson = "Rajesh Kumar",
                        ContactEmail = "procurement@mponline.com"
                    });
                    _db.SaveChanges();
                }

                // Seed Categories
                if (!_db.Categories.Any())
                {
                    _db.Categories.AddRange(
                        new Category { Name = "Dairy & Eggs", Description = "Fresh Milk, Butter, Curd & Eggs", IconCss = "bi-cup-straw" },
                        new Category { Name = "Munchies & Snacks", Description = "Chips, Kurkure, Biscuits & Namkeen", IconCss = "bi-cookie" },
                        new Category { Name = "Cold Drinks & Juices", Description = "Sodas, Energy Drinks & Juices", IconCss = "bi-cup-hot" },
                        new Category { Name = "Instant Food & Staples", Description = "Noodles, Pasta, Rice & Dal", IconCss = "bi-fire" }
                    );
                    _db.SaveChanges();
                }

                // Seed Products
                if (!_db.Products.Any())
                {
                    var dairyCat = _db.Categories.FirstOrDefault(c => c.Name == "Dairy & Eggs")?.CategoryId ?? 1;
                    var snackCat = _db.Categories.FirstOrDefault(c => c.Name == "Munchies & Snacks")?.CategoryId ?? 2;
                    var bevCat = _db.Categories.FirstOrDefault(c => c.Name == "Cold Drinks & Juices")?.CategoryId ?? 3;
                    var instCat = _db.Categories.FirstOrDefault(c => c.Name == "Instant Food & Staples")?.CategoryId ?? 4;

                    _db.Products.AddRange(
                        new Product { SKU = "MPO-DRY-101", Name = "Amul Taaza T-Special Milk 500ml", Price = 27.00m, QuantityOnHand = 85, MinStockLevel = 20, AisleLocation = "Aisle A-1, Bin 04", CategoryId = dairyCat, ExpiryDate = DateTime.Now.AddDays(3) },
                        new Product { SKU = "MPO-DRY-102", Name = "Nandini GoodLife Toned Milk 1L", Price = 56.00m, QuantityOnHand = 40, MinStockLevel = 15, AisleLocation = "Aisle A-1, Bin 05", CategoryId = dairyCat, ExpiryDate = DateTime.Now.AddDays(14) },
                        new Product { SKU = "MPO-DRY-103", Name = "Fresh Farm Eggs 6 Pack", Price = 48.00m, QuantityOnHand = 35, MinStockLevel = 10, AisleLocation = "Aisle A-2, Bin 01", CategoryId = dairyCat, ExpiryDate = DateTime.Now.AddDays(10) },
                        new Product { SKU = "MPO-DRY-104", Name = "Epigamia Greek Yogurt Strawberry 85g", Price = 60.00m, QuantityOnHand = 6, MinStockLevel = 15, AisleLocation = "Aisle A-2, Bin 09", CategoryId = dairyCat, ExpiryDate = DateTime.Now.AddDays(5) },
                        new Product { SKU = "MPO-MNC-201", Name = "Lays India's Magic Masala Chips 50g", Price = 20.00m, QuantityOnHand = 150, MinStockLevel = 30, AisleLocation = "Aisle B-3, Bin 12", CategoryId = snackCat, ExpiryDate = DateTime.Now.AddMonths(4) },
                        new Product { SKU = "MPO-MNC-202", Name = "Kurkure Masala Munch 85g", Price = 20.00m, QuantityOnHand = 120, MinStockLevel = 25, AisleLocation = "Aisle B-3, Bin 14", CategoryId = snackCat, ExpiryDate = DateTime.Now.AddMonths(4) },
                        new Product { SKU = "MPO-BEV-301", Name = "Coca-Cola Soft Drink 750ml", Price = 45.00m, QuantityOnHand = 90, MinStockLevel = 20, AisleLocation = "Aisle C-2, Bin 02", CategoryId = bevCat, ExpiryDate = DateTime.Now.AddMonths(6) },
                        new Product { SKU = "MPO-BEV-302", Name = "Red Bull Energy Drink 250ml", Price = 125.00m, QuantityOnHand = 60, MinStockLevel = 15, AisleLocation = "Aisle C-2, Bin 08", CategoryId = bevCat, ExpiryDate = DateTime.Now.AddMonths(12) },
                        new Product { SKU = "MPO-INS-401", Name = "Maggi 2-Minute Masala Noodles 280g (4-Pack)", Price = 58.00m, QuantityOnHand = 200, MinStockLevel = 40, AisleLocation = "Aisle D-1, Bin 01", CategoryId = instCat, ExpiryDate = DateTime.Now.AddMonths(8) }
                    );
                    _db.SaveChanges();
                }

                // Seed Initial Debug Event Logs
                if (!_db.DebugEventLogs.Any())
                {
                    _db.DebugEventLogs.AddRange(
                        new DebugEventLog { Timestamp = DateTime.Now.AddMinutes(-25), EventType = "SystemInit", Message = "MP Online Dark Store Engine Initialized.", DetailsJson = "{\"StoreId\":1,\"Status\":\"Online\"}" },
                        new DebugEventLog { Timestamp = DateTime.Now.AddMinutes(-10), EventType = "LowStockAlert", Message = "Low stock alert triggered for Epigamia Greek Yogurt (6 units remaining, min: 15).", DetailsJson = "{\"SKU\":\"MPO-DRY-104\",\"CurrentStock\":6}" }
                    );
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message}");
            }
        }
    }
}