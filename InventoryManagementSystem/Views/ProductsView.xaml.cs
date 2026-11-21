
using System;
using InventoryManagementSystem.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace InventoryManagementSystem.Views
{
    public partial class ProductsView : UserControl
    {
        private readonly ObservableCollection<Product> products = new();
        private ICollectionView? productsView;
        private readonly ProductService productService = new();
        private readonly CategoryService categoryService = new();

        public ProductsView()
        {
            InitializeComponent();
            LoadProducts();
        }

        // LOAD PRODUCTS


        private void LoadProducts()
        {
            // Clear existing items
            products.Clear();

            // Read products from SQL Server
            var databaseProducts = productService.GetAllProducts();

            foreach (var product in databaseProducts)
            {
                products.Add(new Product
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Category = product.Category.Name,
                    Quantity = product.Quantity,
                    Price = (double)product.Price
                });
            }

            // Configure search and filtering
            productsView = CollectionViewSource.GetDefaultView(products);
            productsView.Filter = FilterProducts;

            // Show products in the table
            ProductsDataGrid.ItemsSource = productsView;

            // Update statistics
            UpdateStatistics();
        }


        // SEARCH AND CATEGORY FILTER

        private bool FilterProducts(object item)
        {
            if (item is not Product product)
                return false;

            string search = SearchBox.Text?.Trim() ?? "";

            bool matchesSearch =
                product.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                product.Category.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase
                );

            string category = "All Categories";

            if (CategoryComboBox.SelectedItem is ComboBoxItem selected)
            {
                category = selected.Content?.ToString()
                           ?? "All Categories";
            }

            bool matchesCategory =
                category == "All Categories" ||
                product.Category == category;

            return matchesSearch && matchesCategory;
        }

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            productsView?.Refresh();
        }

        private void CategoryComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            productsView?.Refresh();
        }

        // UPDATE STATISTICS

        private void UpdateStatistics()
        {
            TotalProductsText.Text = products.Count.ToString();

            InStockText.Text = products
                .Count(p => p.Quantity > 10)
                .ToString();

            LowStockText.Text = products
                .Count(p => p.Quantity <= 10)
                .ToString();

            CategoriesText.Text = products
                .Select(p => p.Category)
                .Distinct()
                .Count()
                .ToString();
        }

        // ADD PRODUCT


        private void AddProductButton_Click(
            object sender, RoutedEventArgs e)
        {
            Product? newProduct = ShowProductDialog(null);

            if (newProduct == null)
                return;

            try
            {
                // Find the selected category in SQL Server
                var category = categoryService.GetAllCategories()
                    .FirstOrDefault(c =>
                        c.Name.Equals(
                            newProduct.Category,
                            StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    MessageBox.Show(
                        "Selected category does not exist. " +
                        "Please add it in Categories first.",
                        "Category Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // CREATE - Save product to SQL Server
                productService.AddProduct(
                    newProduct.Name,
                    category.CategoryId,
                    newProduct.Quantity,
                    Math.Round((decimal)newProduct.Price, 2));

                // Reload products from SQL Server
                LoadProducts();

                MessageBox.Show(
                    "Product added successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding product: {ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // EDIT PRODUCT

        private void EditProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Product selectedProduct)
            {
                return;
            }

            Product? editedProduct =
                ShowProductDialog(selectedProduct);

            if (editedProduct == null)
                return;

            selectedProduct.Name = editedProduct.Name;
            selectedProduct.Category = editedProduct.Category;
            selectedProduct.Quantity = editedProduct.Quantity;
            selectedProduct.Price = editedProduct.Price;

            productsView?.Refresh();
            ProductsDataGrid.Items.Refresh();

            UpdateStatistics();
        }

        // DELETE PRODUCT

        private void DeleteProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Product selectedProduct)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete '{selectedProduct.Name}'?",
                "Delete Product",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            products.Remove(selectedProduct);

            productsView?.Refresh();
            UpdateStatistics();
        }

        // ADD / EDIT PRODUCT DIALOG

        private Product? ShowProductDialog(Product? existingProduct)
        {
            bool isEditing = existingProduct != null;

            Window dialog = new Window
            {
                Title = isEditing
                    ? "Edit Product"
                    : "Add New Product",

                Width = 460,
                Height = 570,

                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner,

                Owner = Window.GetWindow(this),

                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            StackPanel mainPanel = new StackPanel
            {
                Margin = new Thickness(28)
            };

            TextBlock heading = new TextBlock
            {
                Text = isEditing
                    ? "Edit Product"
                    : "Add New Product",

                FontSize = 24,
                FontWeight = FontWeights.Bold,

                Foreground = new SolidColorBrush(
                    Color.FromRgb(25, 40, 63)
                ),

                Margin = new Thickness(0, 0, 0, 6)
            };

            TextBlock subtitle = new TextBlock
            {
                Text = isEditing
                    ? "Update the product information."
                    : "Enter the details of the new inventory item.",

                FontSize = 12,

                Foreground = new SolidColorBrush(
                    Color.FromRgb(137, 149, 168)
                ),

                Margin = new Thickness(0, 0, 0, 22)
            };

            mainPanel.Children.Add(heading);
            mainPanel.Children.Add(subtitle);

            // PRODUCT NAME

            TextBox nameBox = CreateTextBox();
            nameBox.Text = existingProduct?.Name ?? "";

            AddField(mainPanel, "PRODUCT NAME", nameBox);

            // CATEGORY

            ComboBox categoryBox = new ComboBox
            {
                Height = 40,
                FontSize = 13,
                Padding = new Thickness(9),

                Margin = new Thickness(0, 0, 0, 15),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(38, 54, 77)
                )
            };

            categoryBox.Items.Add("Electronics");
            categoryBox.Items.Add("Furniture");
            categoryBox.Items.Add("Accessories");

            categoryBox.SelectedItem =
                existingProduct?.Category ?? "Electronics";

            AddField(mainPanel, "CATEGORY", categoryBox);

            // QUANTITY

            TextBox quantityBox = CreateTextBox();

            quantityBox.Text =
                existingProduct?.Quantity.ToString() ?? "1";

            AddField(mainPanel, "QUANTITY", quantityBox);

            // PRICE

            TextBox priceBox = CreateTextBox();

            priceBox.Text = existingProduct?.Price.ToString(
                "F2",
                CultureInfo.InvariantCulture
            ) ?? "0.00";

            AddField(mainPanel, "UNIT PRICE", priceBox);

            // ERROR MESSAGE

            TextBlock errorText = new TextBlock
            {
                Foreground = Brushes.IndianRed,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            mainPanel.Children.Add(errorText);

            // BUTTONS

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,

                HorizontalAlignment =
                    HorizontalAlignment.Right,

                Margin = new Thickness(0, 10, 0, 0)
            };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 95,
                Height = 40,

                Margin = new Thickness(0, 0, 10, 0),

                Background = new SolidColorBrush(
                    Color.FromRgb(242, 245, 250)
                ),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(52, 64, 84)
                ),

                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            cancelButton.Click += (s, args) =>
            {
                dialog.DialogResult = false;
            };

            Button saveButton = new Button
            {
                Content = isEditing
                    ? "Save Changes"
                    : "Save Product",

                Width = 125,
                Height = 40,

                Background = new SolidColorBrush(
                    Color.FromRgb(65, 105, 225)
                ),

                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),

                Cursor = System.Windows.Input.Cursors.Hand
            };

            saveButton.Click += (s, args) =>
            {
                string name = nameBox.Text.Trim();

                string category =
                    categoryBox.SelectedItem?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(name))
                {
                    errorText.Text =
                        "Please enter a product name.";

                    return;
                }

                if (string.IsNullOrWhiteSpace(category))
                {
                    errorText.Text =
                        "Please select a category.";

                    return;
                }

                if (!int.TryParse(
                    quantityBox.Text.Trim(),
                    out int quantity) || quantity < 0)
                {
                    errorText.Text =
                        "Quantity must be a valid non-negative whole number.";

                    return;
                }

                string priceInput = priceBox.Text.Trim();

                bool validPrice =
                    double.TryParse(
                        priceInput,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out double price
                    ) ||
                    double.TryParse(
                        priceInput,
                        NumberStyles.Number,
                        CultureInfo.CurrentCulture,
                        out price
                    );

                if (!validPrice ||
                    price < 0 ||
                    double.IsNaN(price) ||
                    double.IsInfinity(price))
                {
                    errorText.Text =
                        "Please enter a valid non-negative price.";

                    return;
                }

                Product result = new Product
                {
                    ProductId = existingProduct?.ProductId ?? 0,
                    Name = name,
                    Category = category,
                    Quantity = quantity,
                    Price = price
                };

                dialog.Tag = result;
                dialog.DialogResult = true;
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);

            mainPanel.Children.Add(buttons);

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility =
                    ScrollBarVisibility.Auto
            };

            dialog.Content = scrollViewer;

            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult == true &&
                dialog.Tag is Product savedProduct)
            {
                return savedProduct;
            }

            return null;
        }

        // TEXTBOX DESIGN

        private static TextBox CreateTextBox()
        {
            return new TextBox
            {
                Height = 40,
                FontSize = 13,

                Padding = new Thickness(12, 8, 12, 8),

                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(225, 230, 239)
                ),

                BorderThickness = new Thickness(1),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(38, 54, 77)
                ),

                VerticalContentAlignment =
                    VerticalAlignment.Center,

                Margin = new Thickness(0, 0, 0, 15)
            };
        }

        private static void AddField(
            Panel panel,
            string label,
            Control input)
        {
            TextBlock fieldLabel = new TextBlock
            {
                Text = label,
                FontSize = 10,
                FontWeight = FontWeights.Bold,

                Foreground = new SolidColorBrush(
                    Color.FromRgb(119, 131, 152)
                ),

                Margin = new Thickness(0, 0, 0, 8)
            };

            panel.Children.Add(fieldLabel);
            panel.Children.Add(input);
        }
    }

    // PRODUCT MODEL

    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = "";

        public string Category { get; set; } = "";

        public int Quantity { get; set; }

        public double Price { get; set; }

        public string Status =>
            Quantity <= 10 ? "Low Stock" : "In Stock";
    }
}
