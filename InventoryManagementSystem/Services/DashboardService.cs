
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class DashboardService
    {
        private readonly AppDbContextFactory factory = new();

        // GET REAL DASHBOARD DATA
        public DashboardStats GetDashboardStats()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            // Get product stock quantities
            var quantities = db.Products
                .AsNoTracking()
                .Select(p => p.Quantity)
                .ToList();

            // Get latest 5 stock movements
            var activities = db.StockMovements
                .AsNoTracking()
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.MovementId)
                .Take(5)
                .Select(m => new DashboardActivity
                {
                    ProductName = m.Product.Name,
                    MovementType = m.MovementType,
                    Quantity = m.Quantity,
                    MovementDate = m.MovementDate
                })
                .ToList();

            return new DashboardStats
            {
                TotalProducts = quantities.Count,

                TotalStock = quantities.Sum(q => (long)q),

                LowStockProducts = quantities.Count(q => q <= 10),

                TotalSuppliers = db.Suppliers.Count(),

                RecentActivities = activities
            };
        }
    }

    // DASHBOARD STATISTICS MODEL
    public class DashboardStats
    {
        public int TotalProducts { get; set; }

        public long TotalStock { get; set; }

        public int LowStockProducts { get; set; }

        public int TotalSuppliers { get; set; }

        public List<DashboardActivity> RecentActivities { get; set; }
            = new();
    }

    // RECENT ACTIVITY MODEL
    public class DashboardActivity
    {
        public string ProductName { get; set; } = "";

        public string MovementType { get; set; } = "";

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; }
    }
}
