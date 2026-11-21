
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace InventoryManagementSystem.Views
{
    public partial class ReportsView : UserControl
    {
        private readonly ObservableCollection<InventoryReportItem> products = new();

        public ReportsView()
        {
            InitializeComponent();

            LoadSampleData();
            RefreshReports();
        }

        private void LoadSampleData()
        {
            products.Add(new InventoryReportItem
            {
                ProductName = "Wireless Mouse",
                Category = "Electronics",
                Quantity = 45,
                UnitPrice = 25.00
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "Office Chair",
                Category = "Furniture",
                Quantity = 12,
                UnitPrice = 120.00
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "USB Keyboard",
                Category = "Electronics",
                Quantity = 3,
                UnitPrice = 40.00
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "Monitor",
                Category = "Electronics",
                Quantity = 20,
                UnitPrice = 180.00
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "Desk Lamp",
                Category = "Accessories",
                Quantity = 2,
                UnitPrice = 35.00
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "Laptop Stand",
                Category = "Accessories",
                Quantity = 15,
                UnitPrice = 29.99
            });

            products.Add(new InventoryReportItem
            {
                ProductName = "Mechanical Keyboard",
                Category = "Electronics",
                Quantity = 8,
                UnitPrice = 89.99
            });
        }

        private void RefreshReports()
        {
            foreach (InventoryReportItem product in products)
            {
                product.Status = product.Quantity <= 10
                    ? "Low Stock"
                    : "In Stock";
            }

            InventoryDataGrid.ItemsSource = products;

            LowStockDataGrid.ItemsSource =
                products.Where(p => p.Quantity <= 10).ToList();

            TotalProductsText.Text = products.Count.ToString();

            TotalStockText.Text =
                products.Sum(p => p.Quantity).ToString();

            double totalValue =
                products.Sum(p => p.Quantity * p.UnitPrice);

            StockValueText.Text =
                totalValue.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

            LowStockText.Text =
                products.Count(p => p.Quantity <= 10).ToString();

            // Sample movement totals.
            StockInText.Text = "10";
            StockOutText.Text = "2";
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export Inventory Report",
                FileName = "NEXORA_Inventory_Report.csv",
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "Product,Category,Quantity,Unit Price,Total Value,Status");

                foreach (InventoryReportItem product in products)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(product.ProductName),
                        EscapeCsv(product.Category),
                        product.Quantity.ToString(),
                        product.UnitPrice.ToString(
                            "F2", CultureInfo.InvariantCulture),
                        product.TotalValue.ToString(
                            "F2", CultureInfo.InvariantCulture),
                        EscapeCsv(product.Status)));
                }

                File.WriteAllText(
                    dialog.FileName,
                    csv.ToString(),
                    new UTF8Encoding(true));

                MessageBox.Show(
                    "Inventory report exported successfully!",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not export the report.\n" + ex.Message,
                    "Export Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string EscapeCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }

    public class InventoryReportItem
    {
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalValue => Quantity * UnitPrice;
        public string Status { get; set; } = "";
    }
}
