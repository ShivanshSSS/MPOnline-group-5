using Inventory.DataAccess.Data;
using Inventory.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext db, ILogger<DashboardController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _db.Products.Include(p => p.Category).ToListAsync();
                var orders = await _db.WarehouseOrders.Include(o => o.OrderItems).OrderByDescending(o => o.OrderDate).Take(8).ToListAsync();
                var darkStores = await _db.DarkStores.ToListAsync();

                ViewBag.TotalProducts = products.Count;
                ViewBag.LowStockCount = products.Count(p => p.IsLowStock);
                ViewBag.OutOfStockCount = products.Count(p => p.QuantityOnHand == 0);
                ViewBag.TotalInventoryValue = products.Sum(p => p.Price * p.QuantityOnHand);
                ViewBag.TotalDarkStores = darkStores.Count;
                ViewBag.ActiveOrdersCount = orders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Picking");

                ViewBag.LowStockItems = products.Where(p => p.IsLowStock).OrderBy(p => p.QuantityOnHand).Take(6).ToList();
                ViewBag.RecentOrders = orders;
                ViewBag.Products = products;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Blinkit Admin Dashboard");
                return View();
            }
        }
    }
}
