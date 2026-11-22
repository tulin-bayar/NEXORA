
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly DashboardService dashboardService = new();

        private readonly Brush activeBackground =
            new SolidColorBrush(Color.FromRgb(43, 66, 99));

        private readonly Brush inactiveBackground =
            Brushes.Transparent;

        private readonly Brush activeForeground =
            Brushes.White;

        private readonly Brush inactiveForeground =
            new SolidColorBrush(Color.FromRgb(197, 204, 218));

        public MainWindow()
        {
            InitializeComponent();

            // Load dashboard after the window is ready
            Loaded += (sender, e) =>
            {
                ShowDashboard();
            };
        }

        // =====================================
        // DASHBOARD - REAL SQL SERVER DATA
        // =====================================

        private void LoadDashboardData()
        {
            try
            {
                var stats = dashboardService.GetDashboardStats();

                // REAL STATISTICS

                DashboardTotalProductsText.Text =
                    stats.TotalProducts.ToString(
                        "N0", CultureInfo.InvariantCulture);

                DashboardTotalStockText.Text =
                    stats.TotalStock.ToString(
                        "N0", CultureInfo.InvariantCulture);

                DashboardLowStockText.Text =
                    stats.LowStockProducts.ToString(
                        "N0", CultureInfo.InvariantCulture);

                DashboardSuppliersText.Text =
                    stats.TotalSuppliers.ToString(
                        "N0", CultureInfo.InvariantCulture);

                // REAL RECENT STOCK MOVEMENTS

                var activities = stats.RecentActivities
                    .Select(m => new DashboardActivityRow
                    {
                        Description =
                            $"{m.ProductName} — {m.MovementType}: " +
                            $"{m.Quantity} units",

                        DateLabel = m.MovementDate.ToString(
                            "dd MMM yyyy, HH:mm",
                            CultureInfo.InvariantCulture)
                    })
                    .ToList();

                if (activities.Count == 0)
                {
                    activities.Add(new DashboardActivityRow
                    {
                        Description = "No stock movements recorded yet.",
                        DateLabel = ""
                    });
                }

                RecentActivitiesList.ItemsSource = activities;
            }
            catch (Exception ex)
            {
                EnglishDialog.Alert(
                    this,
                    "Dashboard Error",
                    $"Could not load dashboard data: {ex.Message}");
            }
        }

        // =====================================
        // NAVIGATION BUTTON EVENTS
        // =====================================

        private void DashboardButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void ProductsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowProducts();
        }

        private void CategoriesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowCategories();
        }

        private void StockMovementsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowStockMovements();
        }

        private void SuppliersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowSuppliers();
        }

        private void ReportsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            HideAllPages();

            ReportsPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Reports";

            SetActiveNavigation(ReportsButton);
        }

        private void SettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            HideAllPages();

            SettingsPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Settings";

            SetActiveNavigation(SettingsButton);
        }

        // =====================================
        // SHOW DASHBOARD
        // =====================================

        private void ShowDashboard()
        {
            HideAllPages();

            DashboardView.Visibility = Visibility.Visible;

            PageTitleText.Text = "Dashboard";

            SetActiveNavigation(DashboardButton);

            // Refresh values every time dashboard opens
            LoadDashboardData();
        }

        // =====================================
        // SHOW PRODUCTS
        // =====================================

        private void ShowProducts()
        {
            HideAllPages();

            ProductsPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Products";

            SetActiveNavigation(ProductsButton);
        }

        // =====================================
        // SHOW CATEGORIES
        // =====================================

        private void ShowCategories()
        {
            HideAllPages();

            CategoriesPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Categories";

            SetActiveNavigation(CategoriesButton);
        }

        // =====================================
        // SHOW STOCK MOVEMENTS
        // =====================================

        private void ShowStockMovements()
        {
            HideAllPages();

            StockMovementsPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Stock Movements";

            SetActiveNavigation(StockMovementsButton);
        }

        // =====================================
        // SHOW SUPPLIERS
        // =====================================

        private void ShowSuppliers()
        {
            HideAllPages();

            SuppliersPage.Visibility = Visibility.Visible;

            PageTitleText.Text = "Suppliers";

            SetActiveNavigation(SuppliersButton);
        }

        // =====================================
        // HIDE ALL PAGES
        // =====================================

        private void HideAllPages()
        {
            DashboardView.Visibility = Visibility.Collapsed;

            ProductsPage.Visibility = Visibility.Collapsed;

            CategoriesPage.Visibility = Visibility.Collapsed;

            StockMovementsPage.Visibility = Visibility.Collapsed;

            SuppliersPage.Visibility = Visibility.Collapsed;

            ReportsPage.Visibility = Visibility.Collapsed;

            SettingsPage.Visibility = Visibility.Collapsed;
        }

        // =====================================
        // ACTIVE SIDEBAR BUTTON
        // =====================================

        private void SetActiveNavigation(Button activeButton)
        {
            DashboardButton.Background = inactiveBackground;
            DashboardButton.Foreground = inactiveForeground;

            ProductsButton.Background = inactiveBackground;
            ProductsButton.Foreground = inactiveForeground;

            CategoriesButton.Background = inactiveBackground;
            CategoriesButton.Foreground = inactiveForeground;

            StockMovementsButton.Background = inactiveBackground;
            StockMovementsButton.Foreground = inactiveForeground;

            SuppliersButton.Background = inactiveBackground;
            SuppliersButton.Foreground = inactiveForeground;

            ReportsButton.Background = inactiveBackground;
            ReportsButton.Foreground = inactiveForeground;

            SettingsButton.Background = inactiveBackground;
            SettingsButton.Foreground = inactiveForeground;

            activeButton.Background = activeBackground;
            activeButton.Foreground = activeForeground;
        }
    }

    // =====================================
    // DASHBOARD ACTIVITY DISPLAY MODEL
    // =====================================

    public class DashboardActivityRow
    {
        public string Description { get; set; } = "";

        public string DateLabel { get; set; } = "";
    }
}
