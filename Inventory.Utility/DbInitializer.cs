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
                EnsureUser("supplier@mponline.com", "MP Online Procurement Vendor", "Supplier123!", SD.Role_Supplier);
                EnsureUser("handler@mponline.com", "MP Online Supply Picker", "Handler123!", SD.Role_SupplyHandler);

                // 1. Seed Dark Stores
                if (!_db.DarkStores.Any())
                {
                    _db.DarkStores.AddRange(
                        new DarkStore { StoreName = "MP Online Dark Store - Indiranagar Hub", City = "Bengaluru", Address = "100 Feet Rd, Indiranagar", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Koramangala Hub", City = "Bengaluru", Address = "80 Feet Rd, Koramangala 4th Block", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Whitefield Hub", City = "Bengaluru", Address = "ITPL Main Rd, Whitefield", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Cyber City Hub", City = "Gurugram", Address = "DLF Phase 2, Cyber City", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Powai Central Hub", City = "Mumbai", Address = "Hiranandani Gardens, Powai", IsActive = true },
                        new DarkStore { StoreName = "MP Online Dark Store - Connaught Place Hub", City = "New Delhi", Address = "Block C, Connaught Place", IsActive = true }
                    );
                    _db.SaveChanges();
                }

                // 2. Seed Suppliers
                if (!_db.Suppliers.Any())
                {
                    _db.Suppliers.AddRange(
                        new Supplier { SupplierName = "MP Online National Procurement Ltd", ContactPerson = "Rajesh Kumar", ContactEmail = "procurement@mponline.com" },
                        new Supplier { SupplierName = "Amul Dairy Cooperatives India", ContactPerson = "Suresh Patel", ContactEmail = "orders@amul.coop" },
                        new Supplier { SupplierName = "PepsiCo & Frito-Lay India Ltd", ContactPerson = "Ananya Sharma", ContactEmail = "supply@pepsico.com" },
                        new Supplier { SupplierName = "Nestle India Manufacturing", ContactPerson = "Vikram Malhotra", ContactEmail = "b2b@nestle.in" },
                        new Supplier { SupplierName = "Unilever Consumer Goods Ltd", ContactPerson = "Pooja Verma", ContactEmail = "distribution@unilever.com" }
                    );
                    _db.SaveChanges();
                }

                // Fetch seeded supplier ID
                int defaultSupplierId = _db.Suppliers.Select(s => s.SupplierID).FirstOrDefault();
                if (defaultSupplierId == 0) defaultSupplierId = 1;

                // 3. Seed Categories
                if (!_db.Categories.Any())
                {
                    _db.Categories.AddRange(
                        new Category { Name = "Dairy & Eggs", Description = "Fresh Milk, Butter, Curd, Cheese & Eggs", IconCss = "bi-cup-straw" },
                        new Category { Name = "Munchies & Snacks", Description = "Chips, Kurkure, Biscuits, Namkeen & Sweets", IconCss = "bi-cookie" },
                        new Category { Name = "Cold Drinks & Juices", Description = "Sodas, Energy Drinks, Fresh Juices & Water", IconCss = "bi-cup-hot" },
                        new Category { Name = "Instant Food & Staples", Description = "Noodles, Pasta, Atta, Rice, Dal & Spices", IconCss = "bi-fire" },
                        new Category { Name = "Fresh Fruits & Veggies", Description = "Farm-fresh Produce, Apples, Bananas & Tomatoes", IconCss = "bi-apple" },
                        new Category { Name = "Personal Care & Home", Description = "Cleaners, Detergents, Soaps & Hygiene Supplies", IconCss = "bi-house-heart" }
                    );
                    _db.SaveChanges();
                }

                // 4. Seed Products
                if (!_db.Products.Any())
                {
                    var dairyCat = _db.Categories.FirstOrDefault(c => c.Name == "Dairy & Eggs")?.CategoryId ?? 1;
                    var snackCat = _db.Categories.FirstOrDefault(c => c.Name == "Munchies & Snacks")?.CategoryId ?? 2;
                    var bevCat = _db.Categories.FirstOrDefault(c => c.Name == "Cold Drinks & Juices")?.CategoryId ?? 3;
                    var instCat = _db.Categories.FirstOrDefault(c => c.Name == "Instant Food & Staples")?.CategoryId ?? 4;
                    var vegCat = _db.Categories.FirstOrDefault(c => c.Name == "Fresh Fruits & Veggies")?.CategoryId ?? 5;
                    var homeCat = _db.Categories.FirstOrDefault(c => c.Name == "Personal Care & Home")?.CategoryId ?? 6;

                    _db.Products.AddRange(
                        // Dairy & Eggs
                        new Product { SKU = "MPO-DRY-101", Name = "Amul Taaza T-Special Milk 500ml", Price = 27.00m, QuantityOnHand = 85, MinStockLevel = 20, AisleLocation = "Aisle A-1, Bin 04", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(3) },
                        new Product { SKU = "MPO-DRY-102", Name = "Nandini GoodLife Toned Milk 1L", Price = 56.00m, QuantityOnHand = 40, MinStockLevel = 15, AisleLocation = "Aisle A-1, Bin 05", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(14) },
                        new Product { SKU = "MPO-DRY-103", Name = "Fresh Farm Eggs 6 Pack", Price = 48.00m, QuantityOnHand = 35, MinStockLevel = 10, AisleLocation = "Aisle A-2, Bin 01", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(10) },
                        new Product { SKU = "MPO-DRY-104", Name = "Epigamia Greek Yogurt Strawberry 85g", Price = 60.00m, QuantityOnHand = 6, MinStockLevel = 15, AisleLocation = "Aisle A-2, Bin 09", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(5) },
                        new Product { SKU = "MPO-DRY-105", Name = "Amul Pasteurized Salted Butter 100g", Price = 58.00m, QuantityOnHand = 110, MinStockLevel = 25, AisleLocation = "Aisle A-3, Bin 02", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(45) },
                        new Product { SKU = "MPO-DRY-106", Name = "Milky Mist Fresh Paneer 200g", Price = 95.00m, QuantityOnHand = 4, MinStockLevel = 12, AisleLocation = "Aisle A-3, Bin 07", CategoryId = dairyCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(4) },

                        // Munchies & Snacks
                        new Product { SKU = "MPO-MNC-201", Name = "Lays India's Magic Masala Chips 50g", Price = 20.00m, QuantityOnHand = 150, MinStockLevel = 30, AisleLocation = "Aisle B-1, Bin 12", CategoryId = snackCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(4) },
                        new Product { SKU = "MPO-MNC-202", Name = "Kurkure Masala Munch 85g", Price = 20.00m, QuantityOnHand = 120, MinStockLevel = 25, AisleLocation = "Aisle B-1, Bin 14", CategoryId = snackCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(4) },
                        new Product { SKU = "MPO-MNC-203", Name = "Bingo Tedhe Medhe Masala Tadka 90g", Price = 20.00m, QuantityOnHand = 95, MinStockLevel = 20, AisleLocation = "Aisle B-2, Bin 03", CategoryId = snackCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(5) },
                        new Product { SKU = "MPO-MNC-204", Name = "Haldiram's Aloo Bhujia Namkeen 150g", Price = 55.00m, QuantityOnHand = 75, MinStockLevel = 15, AisleLocation = "Aisle B-2, Bin 08", CategoryId = snackCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(6) },
                        new Product { SKU = "MPO-MNC-205", Name = "Cadbury Oreo Original Vanilla Biscuits 120g", Price = 40.00m, QuantityOnHand = 3, MinStockLevel = 15, AisleLocation = "Aisle B-3, Bin 01", CategoryId = snackCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(8) },

                        // Cold Drinks & Juices
                        new Product { SKU = "MPO-BEV-301", Name = "Coca-Cola Soft Drink 750ml", Price = 45.00m, QuantityOnHand = 90, MinStockLevel = 20, AisleLocation = "Aisle C-1, Bin 02", CategoryId = bevCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(6) },
                        new Product { SKU = "MPO-BEV-302", Name = "Red Bull Energy Drink 250ml", Price = 125.00m, QuantityOnHand = 60, MinStockLevel = 15, AisleLocation = "Aisle C-1, Bin 08", CategoryId = bevCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(12) },
                        new Product { SKU = "MPO-BEV-303", Name = "Tropicana 100% Orange Juice 1L", Price = 140.00m, QuantityOnHand = 30, MinStockLevel = 10, AisleLocation = "Aisle C-2, Bin 04", CategoryId = bevCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(3) },
                        new Product { SKU = "MPO-BEV-304", Name = "Real Fruit Power Mango Juice 1L", Price = 115.00m, QuantityOnHand = 45, MinStockLevel = 12, AisleLocation = "Aisle C-2, Bin 06", CategoryId = bevCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(4) },
                        new Product { SKU = "MPO-BEV-305", Name = "Thums Up Soft Drink 750ml", Price = 45.00m, QuantityOnHand = 110, MinStockLevel = 20, AisleLocation = "Aisle C-3, Bin 01", CategoryId = bevCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(6) },

                        // Instant Food & Staples
                        new Product { SKU = "MPO-INS-401", Name = "Maggi 2-Minute Masala Noodles 280g (4-Pack)", Price = 58.00m, QuantityOnHand = 200, MinStockLevel = 40, AisleLocation = "Aisle D-1, Bin 01", CategoryId = instCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(8) },
                        new Product { SKU = "MPO-INS-402", Name = "Sunfeast Yippee Magic Masala Noodles 240g", Price = 48.00m, QuantityOnHand = 130, MinStockLevel = 30, AisleLocation = "Aisle D-1, Bin 04", CategoryId = instCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(8) },
                        new Product { SKU = "MPO-INS-403", Name = "Fortune Sunlite Refined Sunflower Oil 1L", Price = 135.00m, QuantityOnHand = 50, MinStockLevel = 15, AisleLocation = "Aisle D-2, Bin 02", CategoryId = instCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(12) },
                        new Product { SKU = "MPO-INS-404", Name = "Aashirvaad Shudh Chakki Whole Wheat Atta 5kg", Price = 240.00m, QuantityOnHand = 40, MinStockLevel = 10, AisleLocation = "Aisle D-2, Bin 09", CategoryId = instCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddMonths(6) },

                        // Fresh Produce
                        new Product { SKU = "MPO-VEG-501", Name = "Fresh Robusta Bananas 500g", Price = 32.00m, QuantityOnHand = 65, MinStockLevel = 15, AisleLocation = "Aisle E-1, Bin 01", CategoryId = vegCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(4) },
                        new Product { SKU = "MPO-VEG-502", Name = "Premium Shimla Royal Apples 1kg", Price = 180.00m, QuantityOnHand = 25, MinStockLevel = 10, AisleLocation = "Aisle E-1, Bin 05", CategoryId = vegCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(7) },
                        new Product { SKU = "MPO-VEG-503", Name = "Hybrid Red Tomatoes 1kg", Price = 38.00m, QuantityOnHand = 80, MinStockLevel = 20, AisleLocation = "Aisle E-2, Bin 03", CategoryId = vegCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddDays(5) },

                        // Personal Care & Home
                        new Product { SKU = "MPO-HOM-601", Name = "Dettol Antiseptic Disinfectant Liquid 250ml", Price = 118.00m, QuantityOnHand = 45, MinStockLevel = 10, AisleLocation = "Aisle F-1, Bin 02", CategoryId = homeCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddYears(2) },
                        new Product { SKU = "MPO-HOM-602", Name = "Surf Excel Easy Wash Detergent Powder 1kg", Price = 145.00m, QuantityOnHand = 60, MinStockLevel = 15, AisleLocation = "Aisle F-1, Bin 07", CategoryId = homeCat, SupplierID = defaultSupplierId, ExpiryDate = DateTime.Now.AddYears(3) }
                    );
                    _db.SaveChanges();
                }

                // 5. Seed LabSupplies (for Stock Adjustment & LabSupplies Controllers)
                if (!_db.LabSupplies.Any())
                {
                    var products = _db.Products.ToList();
                    foreach (var p in products)
                    {
                        _db.LabSupplies.Add(new LabSupply
                        {
                            SupplyName = $"{p.Name} [{p.SKU}]",
                            QuantityOnHand = p.QuantityOnHand,
                            ReorderPoint = p.MinStockLevel,
                            SupplierID = p.SupplierID ?? defaultSupplierId
                        });
                    }
                    _db.SaveChanges();
                }

                // 6. Seed Warehouse Orders
                if (!_db.WarehouseOrders.Any())
                {
                    var p1 = _db.Products.FirstOrDefault(p => p.SKU == "MPO-DRY-101");
                    var p2 = _db.Products.FirstOrDefault(p => p.SKU == "MPO-BEV-301");
                    var p3 = _db.Products.FirstOrDefault(p => p.SKU == "MPO-INS-401");
                    var p4 = _db.Products.FirstOrDefault(p => p.SKU == "MPO-MNC-201");
                    var storeId = _db.DarkStores.FirstOrDefault()?.DarkStoreId ?? 1;

                    _db.WarehouseOrders.AddRange(
                        new WarehouseOrder
                        {
                            OrderCode = "MPO-ORD-1001",
                            CustomerName = "Anish Verma",
                            DeliveryAddress = "Flat 402, Sunshine Heights, Indiranagar, Bengaluru",
                            OrderDate = DateTime.Now.AddMinutes(-45),
                            OrderStatus = "Dispatched",
                            TotalAmount = 198.00m,
                            DarkStoreId = storeId,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = p1?.ProductId ?? 1, Quantity = 2, UnitPrice = 27.00m },
                                new OrderItem { ProductId = p3?.ProductId ?? 3, Quantity = 2, UnitPrice = 58.00m }
                            }
                        },
                        new WarehouseOrder
                        {
                            OrderCode = "MPO-ORD-1002",
                            CustomerName = "Priya Sundaram",
                            DeliveryAddress = "Villa 14, Palm Meadows, Whitefield, Bengaluru",
                            OrderDate = DateTime.Now.AddMinutes(-30),
                            OrderStatus = "Picking",
                            TotalAmount = 145.00m,
                            DarkStoreId = storeId,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = p2?.ProductId ?? 2, Quantity = 2, UnitPrice = 45.00m },
                                new OrderItem { ProductId = p4?.ProductId ?? 4, Quantity = 2, UnitPrice = 20.00m }
                            }
                        }
                    );
                    _db.SaveChanges();
                }

                // 7. Seed Debug Event Logs
                if (!_db.DebugEventLogs.Any())
                {
                    _db.DebugEventLogs.AddRange(
                        new DebugEventLog { Timestamp = DateTime.Now.AddMinutes(-60), EventType = "SystemInit", Message = "MP Online Dark Store Engine Initialized.", DetailsJson = "{\"StoreId\":1,\"Status\":\"Online\"}" },
                        new DebugEventLog { Timestamp = DateTime.Now.AddMinutes(-45), EventType = "OrderSimulated", Message = "Order MPO-ORD-1001 dispatched for customer Anish Verma.", DetailsJson = "{\"OrderCode\":\"MPO-ORD-1001\",\"Status\":\"Dispatched\"}" },
                        new DebugEventLog { Timestamp = DateTime.Now.AddMinutes(-30), EventType = "LowStockAlert", Message = "Low stock alert triggered for Epigamia Greek Yogurt (6 units remaining, min: 15).", DetailsJson = "{\"SKU\":\"MPO-DRY-104\",\"CurrentStock\":6}" }
                    );
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }
    }
}