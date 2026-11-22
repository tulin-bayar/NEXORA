
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.Views
{
    public partial class ReportsView : UserControl
    {
        private readonly ObservableCollection<InventoryReportItem>
            products = new();

        private readonly ProductService productService = new();

        private readonly StockMovementService
            movementService = new();

        public ReportsView()
        {
            InitializeComponent();

            InventoryDataGrid.ItemsSource = products;

            // Refresh reports whenever the page opens
            IsVisibleChanged += (sender, e) =>
            {
                if (e.NewValue is bool visible && visible)
                {
                    TryLoadReports();
                }
            };

            Loaded += (sender, e) =>
            {
                if (IsVisible)
                {
                    TryLoadReports();
                }
            };
        }

        // =====================================
        // LOAD REAL DATA FROM SQL SERVER
        // =====================================

        private void TryLoadReports()
        {
            try
            {
                LoadReports();
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    "Could not load reports:\n" + ex.Message);
            }
        }

        private void LoadReports()
        {
            // Get products from SQL Server
            var databaseProducts =
                productService.GetAllProducts();

            // Get stock movements from SQL Server
            var databaseMovements =
                movementService.GetAllMovements();

            products.Clear();

            foreach (var product in databaseProducts)
            {
                products.Add(new InventoryReportItem
                {
                    ProductName = product.Name,

                    Category =
                        product.Category?.Name ?? "Uncategorized",

                    Quantity = product.Quantity,

                    UnitPrice = product.Price,

                    Status = product.Quantity <= 10
                        ? "Low Stock"
                        : "In Stock"
                });
            }

            // =====================================
            // INVENTORY SUMMARY
            // =====================================

            InventoryDataGrid.ItemsSource = products;

            TotalProductsText.Text =
                products.Count.ToString();

            long totalStock =
                products.Sum(p => (long)p.Quantity);

            TotalStockText.Text =
                totalStock.ToString();

            decimal totalValue =
                products.Sum(p => p.TotalValue);

            StockValueText.Text =
                totalValue.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("en-US"));

            // =====================================
            // LOW STOCK REPORT
            // =====================================

            var lowStockProducts = products
                .Where(p => p.Quantity <= 10)
                .ToList();

            LowStockDataGrid.ItemsSource =
                lowStockProducts;

            LowStockText.Text =
                lowStockProducts.Count.ToString();

            // =====================================
            // STOCK MOVEMENT SUMMARY
            // =====================================

            long totalStockIn = databaseMovements
                .Where(m => m.MovementType == "Stock In")
                .Sum(m => (long)m.Quantity);

            long totalStockOut = databaseMovements
                .Where(m => m.MovementType == "Stock Out")
                .Sum(m => (long)m.Quantity);

            StockInText.Text =
                totalStockIn.ToString();

            StockOutText.Text =
                totalStockOut.ToString();
        }

        // =====================================
        // EXPORT CSV
        // =====================================

        private void ExportButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // Refresh before exporting
                LoadReports();

                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Export Inventory Report",

                    FileName = "NEXORA_Inventory_Report.csv",

                    Filter = "CSV Files (*.csv)|*.csv",

                    DefaultExt = ".csv",

                    AddExtension = true,

                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                StringBuilder csv = new StringBuilder();

                // CSV COLUMN HEADERS
                csv.AppendLine(
                    "Product,Category,Quantity,Unit Price,Total Value,Status");

                // REAL DATABASE PRODUCTS
                foreach (var product in products)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(product.ProductName),

                        EscapeCsv(product.Category),

                        product.Quantity.ToString(
                            CultureInfo.InvariantCulture),

                        product.UnitPrice.ToString(
                            "F2",
                            CultureInfo.InvariantCulture),

                        product.TotalValue.ToString(
                            "F2",
                            CultureInfo.InvariantCulture),

                        EscapeCsv(product.Status)
                    ));
                }

                // Save as UTF-8 for Excel compatibility
                File.WriteAllText(
                    dialog.FileName,
                    csv.ToString(),
                    new UTF8Encoding(true));

                ShowMessage(
                    "Export Complete",
                    "Inventory report exported successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Export Error",
                    "Could not export the report:\n" +
                    ex.Message);
            }
        }

        // =====================================
        // CSV TEXT ESCAPING
        // =====================================

        private static string EscapeCsv(string value)
        {
            string text = value ?? "";

            // Prevent spreadsheet formula interpretation
            string trimmed = text.TrimStart();

            if (trimmed.Length > 0 &&
                "=+-@".Contains(trimmed[0]))
            {
                text = "'" + text;
            }

            return "\"" +
                text.Replace("\"", "\"\"") +
                "\"";
        }

        // =====================================
        // ENGLISH MESSAGE WINDOW
        // =====================================

        private void ShowMessage(
            string title,
            string message)
        {
            EnglishDialog.Alert(
                Window.GetWindow(this),
                title,
                message);
        }
    }

    // =====================================
    // REPORT DISPLAY MODEL
    // =====================================

    public class InventoryReportItem
    {
        public string ProductName { get; set; } = "";

        public string Category { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalValue =>
            Quantity * UnitPrice;

        public string Status { get; set; } = "";
    }
}
