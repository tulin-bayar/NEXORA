
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

        public CategoriesView()
        {
            InitializeComponent();

            categories.Add(new CategoryItem
            {
                CategoryId = 1,
                Name = "Electronics",
                Description = "Electronic devices and equipment",
                ProductCount = 4
            });

            categories.Add(new CategoryItem
            {
                CategoryId = 2,
                Name = "Furniture",
                Description = "Office and home furniture",
                ProductCount = 1
            });

            categories.Add(new CategoryItem
            {
                CategoryId = 3,
                Name = "Accessories",
                Description = "Computer and office accessories",
                ProductCount = 2
            });

            categoriesView = CollectionViewSource.GetDefaultView(categories);

            CategoriesDataGrid.ItemsSource = categoriesView;

            UpdateStatistics();
        }

        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCategoryBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a category name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            foreach (var category in categories)
            {
                if (category.Name.Equals(name,
                    System.StringComparison.OrdinalIgnoreCase)
                    && category != selectedCategory)
                {
                    MessageBox.Show("This category already exists.");
                    return;
                }
            }

            if (selectedCategory == null)
            {
                int nextId = categories.Count == 0
                    ? 1
                    : categories.Max(c => c.CategoryId) + 1;

                categories.Add(new CategoryItem
                {
                    CategoryId = nextId,
                    Name = name,
                    Description = description,
                    ProductCount = 0
                });
            }
            else
            {
                selectedCategory.Name = name;
                selectedCategory.Description = description;
            }

            categoriesView.Refresh();

            ResetForm();
            UpdateStatistics();
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

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is CategoryItem category)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete '{category.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    categories.Remove(category);

                    ResetForm();
                    UpdateStatistics();
                }
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
