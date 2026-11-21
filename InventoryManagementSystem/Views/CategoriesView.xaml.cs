
using InventoryManagementSystem.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventoryManagementSystem.Views
{
    public partial class CategoriesView : UserControl
    {
        private ObservableCollection<CategoryItem> categories =
            new ObservableCollection<CategoryItem>();

        private ICollectionView categoriesView;

        private CategoryItem? selectedCategory;
        private readonly CategoryService categoryService = new();

        public CategoriesView()
        {
            InitializeComponent();

            // Load categories from SQL Server
            LoadCategoriesFromDatabase();

            categoriesView = CollectionViewSource.GetDefaultView(categories);

            CategoriesDataGrid.ItemsSource = categoriesView;

            UpdateStatistics();
        }

        private void LoadCategoriesFromDatabase()
        {
            categories.Clear();

            var databaseCategories =
                categoryService.GetAllCategories();

            foreach (var category in databaseCategories)
            {
                categories.Add(new CategoryItem
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = category.Products.Count
                });
            }
        }



        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCategoryBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();

            // Validate category name
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a category name.");
                return;
            }

            // Check for duplicate categories
            bool exists = categories.Any(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && c.CategoryId != (selectedCategory?.CategoryId ?? -1));

            if (exists)
            {
                MessageBox.Show("This category already exists.");
                return;
            }

            try
            {
                if (selectedCategory == null)
                {
                    // CREATE - Save new category to SQL Server
                    categoryService.AddCategory(name, description);
                }
                else
                {
                    // UPDATE - Save changes to SQL Server
                    categoryService.UpdateCategory(
                        selectedCategory.CategoryId,
                        name,
                        description);
                }

                // Reload categories from database
                LoadCategoriesFromDatabase();

                categoriesView.Refresh();

                ResetForm();
                UpdateStatistics();

                MessageBox.Show(
                    "Category saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving category: {ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is CategoryItem category)
            {
                selectedCategory = category;

                NewCategoryBox.Text = category.Name;
                DescriptionBox.Text = category.Description;

                FormTitleText.Text = "Edit Category";
                SaveCategoryButton.Content = "Update Category";
                CancelEditButton.Visibility = Visibility.Visible;
            }
        }


        private void DeleteCategoryButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not CategoryItem category)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete '{category.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Delete category from SQL Server
                categoryService.DeleteCategory(category.CategoryId);

                // Reload categories from database
                LoadCategoriesFromDatabase();

                categoriesView.Refresh();

                ResetForm();
                UpdateStatistics();

                MessageBox.Show(
                    "Category deleted successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                MessageBox.Show(
                    "This category cannot be deleted because it has related products.",
                    "Delete Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting category: {ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (categoriesView == null)
                return;

            string search = SearchBox.Text.Trim().ToLower();

            categoriesView.Filter = item =>
            {
                if (item is not CategoryItem category)
                    return false;

                return category.Name.ToLower().Contains(search)
                    || category.Description.ToLower().Contains(search);
            };
        }

        private void ResetForm()
        {
            selectedCategory = null;

            NewCategoryBox.Clear();
            DescriptionBox.Clear();

            FormTitleText.Text = "Add New Category";
            SaveCategoryButton.Content = "Save Category";

            CancelEditButton.Visibility = Visibility.Collapsed;
        }

        private void UpdateStatistics()
        {
            TotalCategoriesText.Text = categories.Count.ToString();

            TotalProductsText.Text = categories.Sum(c => c.ProductCount).ToString();
        }
    }

    public class CategoryItem : INotifyPropertyChanged
    {
        private string name = "";
        private string description = "";
        private int productCount;

        public int CategoryId { get; set; }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public int ProductCount
        {
            get => productCount;
            set
            {
                productCount = value;
                OnPropertyChanged(nameof(ProductCount));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
