
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Services;

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

            LoadCategoryFilter();
            LoadProducts();

            IsVisibleChanged += (sender, e) =>
            {
                if (e.NewValue is bool visible && visible)
                {
                    try
                    {
                        LoadCategoryFilter();
                        LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Database Error", ex.Message);
                    }
                }
            };
        }

        // ======================================
        // LOAD CATEGORIES FROM SQL SERVER
        // ======================================

        private void LoadCategoryFilter()
        {
            CategoryComboBox.Items.Clear();

            CategoryComboBox.Items.Add(new ComboBoxItem
            {
                Content = "All Categories"
            });

            var categories = categoryService.GetAllCategories();

            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = category.Name
                });
            }

            CategoryComboBox.SelectedIndex = 0;
        }

        // ======================================
        // LOAD PRODUCTS FROM SQL SERVER
        // ======================================

        private void LoadProducts()
        {
            products.Clear();

            var databaseProducts = productService.GetAllProducts();

            foreach (var product in databaseProducts)
            {
                products.Add(new Product
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Category = product.Category?.Name ?? "",
                    Quantity = product.Quantity,
                    Price = (double)product.Price
                });
            }

            productsView =
                CollectionViewSource.GetDefaultView(products);

            productsView.Filter = FilterProducts;

            ProductsDataGrid.ItemsSource = productsView;

            UpdateStatistics();
        }

        // ======================================
        // SEARCH AND CATEGORY FILTER
        // ======================================

        private bool FilterProducts(object item)
        {
            if (item is not Product product)
                return false;

            string search = SearchBox.Text?.Trim() ?? "";

            bool matchesSearch =
                product.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                product.Category.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);

            string selectedCategory = "All Categories";

            if (CategoryComboBox.SelectedItem is ComboBoxItem selected)
            {
                selectedCategory =
                    selected.Content?.ToString() ?? "All Categories";
            }

            bool matchesCategory =
                selectedCategory == "All Categories" ||
                product.Category.Equals(
                    selectedCategory,
                    StringComparison.OrdinalIgnoreCase);

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

        // ======================================
        // UPDATE STATISTICS
        // ======================================

        private void UpdateStatistics()
        {
            TotalProductsText.Text =
                products.Count.ToString();

            InStockText.Text = products
                .Count(p => p.Quantity > 10)
                .ToString();

            LowStockText.Text = products
                .Count(p => p.Quantity <= 10)
                .ToString();

            CategoriesText.Text =
                Math.Max(0, CategoryComboBox.Items.Count - 1)
                .ToString();
        }

        // ======================================
        // ENGLISH MESSAGE WINDOW
        // ======================================

        private void ShowMessage(string title, string message)
        {
            EnglishDialog.Alert(
                Window.GetWindow(this),
                title,
                message);
        }

        // ======================================
        // ADD PRODUCT - CREATE
        // ======================================

        private void AddProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Product? newProduct = ShowProductDialog(null);

                if (newProduct == null)
                    return;

                var category = categoryService.GetAllCategories()
                    .FirstOrDefault(c =>
                        c.Name.Equals(
                            newProduct.Category,
                            StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    ShowMessage(
                        "Category Error",
                        "Selected category does not exist. " +
                        "Please add it in Categories first.");
                    return;
                }

                productService.AddProduct(
                    newProduct.Name,
                    category.CategoryId,
                    newProduct.Quantity,
                    Math.Round((decimal)newProduct.Price, 2));

                LoadCategoryFilter();
                LoadProducts();

                ShowMessage(
                    "Success",
                    "Product added successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error adding product: {ex.Message}");
            }
        }

        // ======================================
        // EDIT PRODUCT - UPDATE
        // ======================================

        private void EditProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Product selectedProduct)
                return;

            try
            {
                Product? editedProduct =
                    ShowProductDialog(selectedProduct);

                if (editedProduct == null)
                    return;

                var category = categoryService.GetAllCategories()
                    .FirstOrDefault(c =>
                        c.Name.Equals(
                            editedProduct.Category,
                            StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    ShowMessage(
                        "Category Error",
                        "Selected category does not exist.");
                    return;
                }

                productService.UpdateProduct(
                    selectedProduct.ProductId,
                    editedProduct.Name,
                    category.CategoryId,
                    editedProduct.Quantity,
                    Math.Round((decimal)editedProduct.Price, 2));

                LoadCategoryFilter();
                LoadProducts();

                ShowMessage(
                    "Success",
                    "Product updated successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error updating product: {ex.Message}");
            }
        }

        // ======================================
        // DELETE PRODUCT - DELETE
        // ======================================

        private void DeleteProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Product selectedProduct)
                return;

            bool confirmed = EnglishDialog.Confirm(
                Window.GetWindow(this),
                "Delete Product",
                $"Are you sure you want to delete '{selectedProduct.Name}'?");

            if (!confirmed)
                return;

            try
            {
                productService.DeleteProduct(
                    selectedProduct.ProductId);

                LoadCategoryFilter();
                LoadProducts();

                ShowMessage(
                    "Success",
                    "Product deleted successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error deleting product: {ex.Message}");
            }
        }

        // ======================================
        // ADD / EDIT PRODUCT DIALOG
        // ======================================

        private Product? ShowProductDialog(Product? existingProduct)
        {
            bool isEditing = existingProduct != null;

            var categories = categoryService.GetAllCategories();

            if (categories.Count == 0)
            {
                ShowMessage(
                    "No Categories",
                    "Please create a category before adding a product.");
                return null;
            }

            Window? owner = Window.GetWindow(this);

            Window dialog = new Window
            {
                Title = isEditing
                    ? "Edit Product"
                    : "Add New Product",

                Width = 460,
                Height = 570,

                WindowStartupLocation = owner == null
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner,

                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                ShowInTaskbar = false
            };

            if (owner != null)
                dialog.Owner = owner;

            StackPanel mainPanel = new StackPanel
            {
                Margin = new Thickness(28)
            };

            // HEADER

            mainPanel.Children.Add(new TextBlock
            {
                Text = isEditing
                    ? "Edit Product"
                    : "Add New Product",

                FontSize = 24,
                FontWeight = FontWeights.Bold,

                Foreground = new SolidColorBrush(
                    Color.FromRgb(25, 40, 63)),

                Margin = new Thickness(0, 0, 0, 6)
            });

            mainPanel.Children.Add(new TextBlock
            {
                Text = isEditing
                    ? "Update the product information."
                    : "Enter the details of the new inventory item.",

                FontSize = 12,

                Foreground = new SolidColorBrush(
                    Color.FromRgb(137, 149, 168)),

                Margin = new Thickness(0, 0, 0, 22)
            });

            // PRODUCT NAME

            TextBox nameBox = CreateTextBox();

            nameBox.Text = existingProduct?.Name ?? "";

            AddField(mainPanel, "PRODUCT NAME", nameBox);

            // CATEGORY - LOAD FROM DATABASE

            ComboBox categoryBox = new ComboBox
            {
                Height = 40,
                FontSize = 13,
                Padding = new Thickness(9),
                Margin = new Thickness(0, 0, 0, 15),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(38, 54, 77))
            };

            foreach (var category in categories)
            {
                categoryBox.Items.Add(category.Name);
            }

            if (isEditing &&
                categoryBox.Items.Contains(existingProduct!.Category))
            {
                categoryBox.SelectedItem = existingProduct.Category;
            }
            else
            {
                categoryBox.SelectedIndex = 0;
            }

            AddField(mainPanel, "CATEGORY", categoryBox);

            // QUANTITY

            TextBox quantityBox = CreateTextBox();

            quantityBox.Text =
                existingProduct?.Quantity.ToString() ?? "1";

            AddField(mainPanel, "QUANTITY", quantityBox);

            // UNIT PRICE

            TextBox priceBox = CreateTextBox();

            priceBox.Text = existingProduct?.Price.ToString(
                "F2",
                CultureInfo.InvariantCulture) ?? "0.00";

            AddField(mainPanel, "UNIT PRICE", priceBox);

            // VALIDATION MESSAGE

            TextBlock errorText = new TextBlock
            {
                Foreground = Brushes.IndianRed,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            mainPanel.Children.Add(errorText);

            // BUTTON CONTAINER

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // CANCEL BUTTON

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 95,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),

                Background = new SolidColorBrush(
                    Color.FromRgb(242, 245, 250)),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(52, 64, 84)),

                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                IsCancel = true
            };

            cancelButton.Click += (s, args) =>
            {
                dialog.DialogResult = false;
            };

            // SAVE BUTTON

            Button saveButton = new Button
            {
                Content = isEditing
                    ? "Save Changes"
                    : "Save Product",

                Width = 125,
                Height = 40,

                Background = new SolidColorBrush(
                    Color.FromRgb(65, 105, 225)),

                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                IsDefault = true
            };

            // SAVE BUTTON VALIDATION

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

                NumberStyles priceStyle =
                    NumberStyles.AllowDecimalPoint |
                    NumberStyles.AllowLeadingSign;

                bool validPrice =
                    decimal.TryParse(
                        priceInput,
                        priceStyle,
                        CultureInfo.InvariantCulture,
                        out decimal price)
                    ||
                    decimal.TryParse(
                        priceInput,
                        priceStyle,
                        CultureInfo.CurrentCulture,
                        out price);

                if (!validPrice || price < 0)
                {
                    errorText.Text =
                        "Please enter a valid non-negative price.";
                    return;
                }

                if (price > 9999999999999999m)
                {
                    errorText.Text =
                        "The price is too large.";
                    return;
                }

                Product result = new Product
                {
                    ProductId = existingProduct?.ProductId ?? 0,
                    Name = name,
                    Category = category,
                    Quantity = quantity,
                    Price = (double)price
                };

                dialog.Tag = result;
                dialog.DialogResult = true;
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);

            mainPanel.Children.Add(buttons);

            // SCROLLABLE DIALOG CONTENT

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility =
                    ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility =
                    ScrollBarVisibility.Disabled
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

        // ======================================
        // TEXTBOX DESIGN
        // ======================================

        private static TextBox CreateTextBox()
        {
            return new TextBox
            {
                Height = 40,
                FontSize = 13,

                Padding = new Thickness(12, 8, 12, 8),

                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(225, 230, 239)),

                BorderThickness = new Thickness(1),

                Foreground = new SolidColorBrush(
                    Color.FromRgb(38, 54, 77)),

                VerticalContentAlignment =
                    VerticalAlignment.Center,

                Margin = new Thickness(0, 0, 0, 15)
            };
        }

        // ======================================
        // FORM FIELD DESIGN
        // ======================================

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
                    Color.FromRgb(119, 131, 152)),

                Margin = new Thickness(0, 0, 0, 8)
            };

            panel.Children.Add(fieldLabel);
            panel.Children.Add(input);
        }
    }

    // ======================================
    // UI PRODUCT MODEL
    // ======================================

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
