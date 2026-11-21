
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventoryManagementSystem.Views
{
    public partial class StockMovementsView : UserControl
    {
        private readonly ObservableCollection<StockMovement> movements = new();
        private ICollectionView? movementsView;

        public StockMovementsView()
        {
            InitializeComponent();

            MovementsDataGrid.ItemsSource = movements;

            movementsView = CollectionViewSource.GetDefaultView(movements);
            movementsView.Filter = FilterMovements;

            LoadSampleMovements();
            UpdateStatistics();
        }

        private void LoadSampleMovements()
        {
            movements.Add(new StockMovement
            {
                MovementId = 1,
                ProductName = "Wireless Mouse",
                MovementType = "Stock In",
                Quantity = 10,
                MovementDate = DateTime.Now.AddHours(-5),
                Note = "New delivery"
            });

            movements.Add(new StockMovement
            {
                MovementId = 2,
                ProductName = "USB Keyboard",
                MovementType = "Stock Out",
                Quantity = 2,
                MovementDate = DateTime.Now.AddHours(-3),
                Note = "Sales order"
            });

            movements.Add(new StockMovement
            {
                MovementId = 3,
                ProductName = "Office Chair",
                MovementType = "Stock In",
                Quantity = 5,
                MovementDate = DateTime.Now.AddDays(-1),
                Note = "Supplier delivery"
            });
        }

        private bool FilterMovements(object item)
        {
            if (item is not StockMovement movement)
                return false;

            string search = SearchBox.Text?.Trim() ?? "";

            bool matchesSearch = movement.ProductName.Contains(
                search,
                StringComparison.OrdinalIgnoreCase
            );

            string selectedType = "All Movements";

            if (MovementFilter.SelectedItem is ComboBoxItem selected)
            {
                selectedType = selected.Content?.ToString()
                               ?? "All Movements";
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

        private void StockInButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddMovement("Stock In");
        }

        private void StockOutButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddMovement("Stock Out");
        }

        private void AddMovement(string movementType)
        {
            Window dialog = new Window
            {
                Title = movementType,
                Width = 420,
                Height = 430,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            StackPanel panel = new StackPanel
            {
                Margin = new Thickness(25)
            };

            TextBlock heading = new TextBlock
            {
                Text = movementType,
                FontSize = 23,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.DarkSlateGray,
                Margin = new Thickness(0, 0, 0, 20)
            };

            panel.Children.Add(heading);

            TextBox productBox = CreateInput();
            AddField(panel, "PRODUCT NAME", productBox);

            TextBox quantityBox = CreateInput();
            quantityBox.Text = "1";
            AddField(panel, "QUANTITY", quantityBox);

            TextBox noteBox = CreateInput();
            AddField(panel, "NOTE", noteBox);

            TextBlock errorText = new TextBlock
            {
                Foreground = System.Windows.Media.Brushes.IndianRed,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 10)
            };

            panel.Children.Add(errorText);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 90,
                Height = 38,
                Margin = new Thickness(0, 0, 10, 0)
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
            };

            Button saveButton = new Button
            {
                Content = "Save Movement",
                Width = 125,
                Height = 38,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(65, 105, 225)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold
            };

            saveButton.Click += (s, e) =>
            {
                string productName = productBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(productName))
                {
                    errorText.Text = "Please enter a product name.";
                    return;
                }

                if (!int.TryParse(quantityBox.Text, out int quantity) ||
                    quantity <= 0)
                {
                    errorText.Text = "Quantity must be greater than zero.";
                    return;
                }

                dialog.Tag = new StockMovement
                {
                    MovementId = movements.Count == 0
                        ? 1
                        : movements.Max(m => m.MovementId) + 1,

                    ProductName = productName,
                    MovementType = movementType,
                    Quantity = quantity,
                    MovementDate = DateTime.Now,
                    Note = noteBox.Text.Trim()
                };

                dialog.DialogResult = true;
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);

            panel.Children.Add(buttons);

            dialog.Content = panel;

            if (dialog.ShowDialog() == true &&
                dialog.Tag is StockMovement movement)
            {
                movements.Insert(0, movement);

                movementsView?.Refresh();
                UpdateStatistics();
            }
        }

        private static TextBox CreateInput()
        {
            return new TextBox
            {
                Height = 38,
                FontSize = 12,
                Padding = new Thickness(10, 7, 10, 7),
                Margin = new Thickness(0, 0, 0, 14),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(225, 230, 239))
            };
        }

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
                Foreground = System.Windows.Media.Brushes.SlateGray,
                Margin = new Thickness(0, 0, 0, 7)
            });

            panel.Children.Add(input);
        }

        private void UpdateStatistics()
        {
            TotalMovementsText.Text = movements.Count.ToString();

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
