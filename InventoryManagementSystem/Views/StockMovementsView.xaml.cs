
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.Views
{
    public partial class StockMovementsView : UserControl
    {
        private readonly ObservableCollection<StockMovement> movements = new();

        private ICollectionView? movementsView;

        private readonly StockMovementService movementService = new();
        private readonly ProductService productService = new();

        public StockMovementsView()
        {
            InitializeComponent();

            movementsView =
                CollectionViewSource.GetDefaultView(movements);

            movementsView.Filter = FilterMovements;

            MovementsDataGrid.ItemsSource = movementsView;

            try
            {
                LoadMovements();
            }
            catch (Exception ex)
            {
                ShowMessage("Database Error", ex.Message);
            }

            // Refresh when returning to this page
            IsVisibleChanged += (sender, e) =>
            {
                if (e.NewValue is bool visible && visible)
                {
                    try
                    {
                        LoadMovements();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Database Error", ex.Message);
                    }
                }
            };
        }

        // =====================================
        // LOAD MOVEMENTS FROM SQL SERVER
        // =====================================

        private void LoadMovements()
        {
            movements.Clear();

            var databaseMovements =
                movementService.GetAllMovements();

            foreach (var movement in databaseMovements)
            {
                movements.Add(new StockMovement
                {
                    MovementId = movement.MovementId,

                    ProductName =
                        movement.Product?.Name ?? "Unknown Product",

                    MovementType = movement.MovementType,

                    Quantity = movement.Quantity,

                    MovementDate = movement.MovementDate,

                    Note = movement.Note
                });
            }

            movementsView?.Refresh();

            UpdateStatistics();
        }

        // =====================================
        // SEARCH AND FILTER
        // =====================================

        private bool FilterMovements(object item)
        {
            if (item is not StockMovement movement)
                return false;

            string search = SearchBox.Text?.Trim() ?? "";

            bool matchesSearch =
                movement.ProductName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);

            string selectedType = "All Movements";

            if (MovementFilter.SelectedItem is ComboBoxItem selected)
            {
                selectedType =
                    selected.Content?.ToString() ?? "All Movements";
            }

            bool matchesType =
                selectedType == "All Movements" ||
                movement.MovementType == selectedType;

            return matchesSearch && matchesType;
        }

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            movementsView?.Refresh();
        }

        private void MovementFilter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            movementsView?.Refresh();
        }

        // =====================================
        // STOCK IN BUTTON
        // =====================================

        private void StockInButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddMovement("Stock In");
        }

        // =====================================
        // STOCK OUT BUTTON
        // =====================================

        private void StockOutButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddMovement("Stock Out");
        }

        // =====================================
        // ADD STOCK MOVEMENT
        // =====================================

        private void AddMovement(string movementType)
        {
            try
            {
                // Load real products from SQL Server
                var availableProducts =
                    productService.GetAllProducts();

                if (availableProducts.Count == 0)
                {
                    ShowMessage(
                        "No Products",
                        "Please add a product before recording a stock movement.");

                    return;
                }

                Window? owner = Window.GetWindow(this);

                Window dialog = new Window
                {
                    Title = movementType,
                    Width = 440,
                    Height = 490,

                    WindowStartupLocation = owner == null
                        ? WindowStartupLocation.CenterScreen
                        : WindowStartupLocation.CenterOwner,

                    ResizeMode = ResizeMode.NoResize,

                    Background = Brushes.White,

                    ShowInTaskbar = false
                };

                if (owner != null)
                    dialog.Owner = owner;

                StackPanel panel = new StackPanel
                {
                    Margin = new Thickness(25)
                };

                // HEADER

                TextBlock heading = new TextBlock
                {
                    Text = movementType,
                    FontSize = 23,
                    FontWeight = FontWeights.Bold,

                    Foreground = new SolidColorBrush(
                        Color.FromRgb(25, 40, 63)),

                    Margin = new Thickness(0, 0, 0, 8)
                };

                panel.Children.Add(heading);

                TextBlock subtitle = new TextBlock
                {
                    Text = movementType == "Stock In"
                        ? "Record incoming inventory."
                        : "Record outgoing inventory.",

                    FontSize = 12,
                    Foreground = Brushes.SlateGray,

                    Margin = new Thickness(0, 0, 0, 20)
                };

                panel.Children.Add(subtitle);

                // PRODUCT SELECTION

                ComboBox productBox = new ComboBox
                {
                    Height = 40,
                    FontSize = 13,
                    Padding = new Thickness(9),

                    Margin = new Thickness(0, 0, 0, 14),

                    ItemsSource = availableProducts,

                    DisplayMemberPath = "Name",

                    SelectedValuePath = "ProductId"
                };

                AddField(panel, "SELECT PRODUCT", productBox);

                // CURRENT STOCK

                TextBlock currentStockText = new TextBlock
                {
                    FontSize = 12,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(65, 105, 225)),

                    FontWeight = FontWeights.SemiBold,

                    Margin = new Thickness(0, 0, 0, 14)
                };

                panel.Children.Add(currentStockText);

                productBox.SelectionChanged += (s, e) =>
                {
                    if (productBox.SelectedValue is int id)
                    {
                        var product = availableProducts
                            .FirstOrDefault(p => p.ProductId == id);

                        if (product != null)
                        {
                            currentStockText.Text =
                                $"Current stock: {product.Quantity} units";
                        }
                    }
                };

                productBox.SelectedIndex = 0;

                // QUANTITY

                TextBox quantityBox = CreateInput();

                quantityBox.Text = "1";

                AddField(panel, "QUANTITY", quantityBox);

                // NOTE

                TextBox noteBox = CreateInput();

                AddField(panel, "NOTE", noteBox);

                // VALIDATION MESSAGE

                TextBlock errorText = new TextBlock
                {
                    Foreground = Brushes.IndianRed,

                    FontSize = 11,

                    TextWrapping = TextWrapping.Wrap,

                    Margin = new Thickness(0, 0, 0, 12)
                };

                panel.Children.Add(errorText);

                // BUTTON CONTAINER

                StackPanel buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };

                // CANCEL BUTTON

                Button cancelButton = new Button
                {
                    Content = "Cancel",

                    Width = 90,
                    Height = 38,

                    Margin = new Thickness(0, 0, 10, 0),

                    IsCancel = true
                };

                cancelButton.Click += (s, e) =>
                {
                    dialog.DialogResult = false;
                };

                // SAVE BUTTON

                Button saveButton = new Button
                {
                    Content = "Save Movement",

                    Width = 135,
                    Height = 38,

                    Background = new SolidColorBrush(
                        Color.FromRgb(65, 105, 225)),

                    Foreground = Brushes.White,

                    BorderThickness = new Thickness(0),

                    FontWeight = FontWeights.SemiBold,

                    IsDefault = true
                };

                saveButton.Click += (s, e) =>
                {
                    errorText.Text = "";

                    // VALIDATE PRODUCT

                    if (productBox.SelectedValue is not int productId)
                    {
                        errorText.Text =
                            "Please select a product.";

                        return;
                    }

                    // VALIDATE QUANTITY

                    if (!int.TryParse(
                        quantityBox.Text.Trim(),
                        out int quantity) || quantity <= 0)
                    {
                        errorText.Text =
                            "Quantity must be greater than zero.";

                        return;
                    }

                    // SAVE TO SQL SERVER

                    try
                    {
                        movementService.AddMovement(
                            productId,
                            movementType,
                            quantity,
                            noteBox.Text.Trim());

                        dialog.DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        errorText.Text = ex.Message;
                    }
                };

                buttons.Children.Add(cancelButton);

                buttons.Children.Add(saveButton);

                panel.Children.Add(buttons);

                // SCROLLABLE CONTENT

                ScrollViewer scrollViewer = new ScrollViewer
                {
                    Content = panel,

                    VerticalScrollBarVisibility =
                        ScrollBarVisibility.Auto,

                    HorizontalScrollBarVisibility =
                        ScrollBarVisibility.Disabled
                };

                dialog.Content = scrollViewer;

                // SHOW DIALOG

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Refresh movement history
                    LoadMovements();

                    ShowMessage(
                        "Success",
                        "Stock movement saved successfully!");
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error processing stock movement: {ex.Message}");
            }
        }

        // =====================================
        // ENGLISH MESSAGE DIALOG
        // =====================================

        private void ShowMessage(string title, string message)
        {
            EnglishDialog.Alert(
                Window.GetWindow(this),
                title,
                message);
        }

        // =====================================
        // TEXTBOX DESIGN
        // =====================================

        private static TextBox CreateInput()
        {
            return new TextBox
            {
                Height = 38,

                FontSize = 12,

                Padding = new Thickness(10, 7, 10, 7),

                Margin = new Thickness(0, 0, 0, 14),

                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(225, 230, 239)),

                VerticalContentAlignment =
                    VerticalAlignment.Center
            };
        }

        // =====================================
        // FORM FIELD DESIGN
        // =====================================

        private static void AddField(
            Panel panel,
            string label,
            Control input)
        {
            panel.Children.Add(new TextBlock
            {
                Text = label,

                FontSize = 10,

                FontWeight = FontWeights.Bold,

                Foreground = Brushes.SlateGray,

                Margin = new Thickness(0, 0, 0, 7)
            });

            panel.Children.Add(input);
        }

        // =====================================
        // UPDATE STATISTICS
        // =====================================

        private void UpdateStatistics()
        {
            TotalMovementsText.Text =
                movements.Count.ToString();

            StockInText.Text = movements
                .Where(m => m.MovementType == "Stock In")
                .Sum(m => m.Quantity)
                .ToString();

            StockOutText.Text = movements
                .Where(m => m.MovementType == "Stock Out")
                .Sum(m => m.Quantity)
                .ToString();
        }
    }

    // =====================================
    // UI STOCK MOVEMENT MODEL
    // =====================================

    public class StockMovement
    {
        public int MovementId { get; set; }

        public string ProductName { get; set; } = "";

        public string MovementType { get; set; } = "";

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; }

        public string Note { get; set; } = "";
    }
}
