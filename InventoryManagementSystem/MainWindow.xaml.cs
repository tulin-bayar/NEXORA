
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventoryManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly Brush activeBackground =
            new SolidColorBrush(Color.FromRgb(43, 66, 99));

        private readonly Brush inactiveBackground = Brushes.Transparent;

        private readonly Brush activeForeground = Brushes.White;

        private readonly Brush inactiveForeground =
            new SolidColorBrush(Color.FromRgb(197, 204, 218));

        public MainWindow()
        {
            InitializeComponent();
            ShowDashboard();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProducts();
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCategories();
        }

        private void StockMovementsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowStockMovements();
        }

        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSuppliers();
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            ReportsPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Reports";
            SetActiveNavigation(ReportsButton);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            SettingsPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Settings";
            SetActiveNavigation(SettingsButton);
        }

        private void ShowDashboard()
        {
            HideAllPages();
            DashboardView.Visibility = Visibility.Visible;
            PageTitleText.Text = "Dashboard";
            SetActiveNavigation(DashboardButton);
        }

        private void ShowProducts()
        {
            HideAllPages();
            ProductsPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Products";
            SetActiveNavigation(ProductsButton);
        }

        private void ShowCategories()
        {
            HideAllPages();
            CategoriesPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Categories";
            SetActiveNavigation(CategoriesButton);
        }

        private void ShowStockMovements()
        {
            HideAllPages();
            StockMovementsPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Stock Movements";
            SetActiveNavigation(StockMovementsButton);
        }

        private void ShowSuppliers()
        {
            HideAllPages();
            SuppliersPage.Visibility = Visibility.Visible;
            PageTitleText.Text = "Suppliers";
            SetActiveNavigation(SuppliersButton);
        }

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
}
