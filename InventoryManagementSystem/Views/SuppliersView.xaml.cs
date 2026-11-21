
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventoryManagementSystem.Views
{
    public partial class SuppliersView : UserControl
    {
        private readonly ObservableCollection<Supplier> suppliers = new();
        private ICollectionView suppliersView;
        private Supplier? selectedSupplier;
        private int nextSupplierId = 1;

        public SuppliersView()
        {
            InitializeComponent();

            suppliersView = CollectionViewSource.GetDefaultView(suppliers);
            suppliersView.Filter = FilterSuppliers;

            SuppliersDataGrid.ItemsSource = suppliersView;

            LoadSampleSuppliers();
            UpdateStatistics();
            ClearForm();
        }

        private void LoadSampleSuppliers()
        {
            suppliers.Add(new Supplier
            {
                SupplierId = nextSupplierId++,
                CompanyName = "TechWorld Solutions",
                ContactPerson = "James Wilson",
                Phone = "+1 202 555 0145",
                Email = "contact@techworld.com",
                Address = "New York, USA",
                Status = "Active"
            });

            suppliers.Add(new Supplier
            {
                SupplierId = nextSupplierId++,
                CompanyName = "Global Furniture",
                ContactPerson = "Emily Brown",
                Phone = "+1 202 555 0182",
                Email = "info@globalfurniture.com",
                Address = "Chicago, USA",
                Status = "Active"
            });

            suppliers.Add(new Supplier
            {
                SupplierId = nextSupplierId++,
                CompanyName = "Smart Accessories",
                ContactPerson = "Michael Davis",
                Phone = "+1 202 555 0193",
                Email = "sales@smartaccessories.com",
                Address = "Boston, USA",
                Status = "Inactive"
            });

            suppliers.Add(new Supplier
            {
                SupplierId = nextSupplierId++,
                CompanyName = "Digital Systems",
                ContactPerson = "Sophia Miller",
                Phone = "+1 202 555 0167",
                Email = "support@digitalsystems.com",
                Address = "Los Angeles, USA",
                Status = "Active"
            });
        }

        private bool FilterSuppliers(object item)
        {
            if (item is not Supplier supplier)
                return false;

            string searchText = SearchBox?.Text?.Trim() ?? "";

            bool matchesSearch =
                supplier.CompanyName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                supplier.ContactPerson.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                supplier.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                supplier.Phone.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            string selectedStatus =
                (StatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString()
                ?? "All Suppliers";

            bool matchesStatus =
                selectedStatus == "All Suppliers" ||
                supplier.Status == selectedStatus;

            return matchesSearch && matchesStatus;
        }

        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            CompanyNameBox.Focus();
        }

        private void SaveSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            string companyName = CompanyNameBox.Text.Trim();
            string contactPerson = ContactPersonBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string address = AddressBox.Text.Trim();

            string status =
                (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                ?? "Active";

            if (string.IsNullOrWhiteSpace(companyName))
            {
                MessageBox.Show(
                    "Please enter the company name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CompanyNameBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contactPerson) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Please complete the contact person, phone and email fields.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!email.Contains('@') || !email.Contains('.'))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Invalid Email",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                EmailBox.Focus();
                return;
            }

            if (selectedSupplier == null)
            {
                Supplier newSupplier = new Supplier
                {
                    SupplierId = nextSupplierId++,
                    CompanyName = companyName,
                    ContactPerson = contactPerson,
                    Phone = phone,
                    Email = email,
                    Address = address,
                    Status = status
                };

                suppliers.Add(newSupplier);
            }
            else
            {
                selectedSupplier.CompanyName = companyName;
                selectedSupplier.ContactPerson = contactPerson;
                selectedSupplier.Phone = phone;
                selectedSupplier.Email = email;
                selectedSupplier.Address = address;
                selectedSupplier.Status = status;
            }

            suppliersView.Refresh();
            UpdateStatistics();
            ClearForm();
        }

        private void EditSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Supplier supplier)
                return;

            selectedSupplier = supplier;

            CompanyNameBox.Text = supplier.CompanyName;
            ContactPersonBox.Text = supplier.ContactPerson;
            PhoneBox.Text = supplier.Phone;
            EmailBox.Text = supplier.Email;
            AddressBox.Text = supplier.Address;

            StatusBox.SelectedIndex =
                supplier.Status == "Inactive" ? 1 : 0;

            FormTitleText.Text = "Edit Supplier Information";
            SaveSupplierButton.Content = "Update Supplier";
            CancelEditButton.Visibility = Visibility.Visible;
        }

        private void DeleteSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Supplier supplier)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete {supplier.CompanyName}?",
                "Delete Supplier",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                suppliers.Remove(supplier);

                if (selectedSupplier == supplier)
                    ClearForm();

                suppliersView.Refresh();
                UpdateStatistics();
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            suppliersView?.Refresh();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            suppliersView?.Refresh();
        }

        private void ClearForm()
        {
            selectedSupplier = null;

            CompanyNameBox.Clear();
            ContactPersonBox.Clear();
            PhoneBox.Clear();
            EmailBox.Clear();
            AddressBox.Clear();

            StatusBox.SelectedIndex = 0;

            FormTitleText.Text = "Supplier Information";
            SaveSupplierButton.Content = "Save Supplier";
            CancelEditButton.Visibility = Visibility.Collapsed;
        }

        private void UpdateStatistics()
        {
            TotalSuppliersText.Text = suppliers.Count.ToString();

            ActiveSuppliersText.Text =
                suppliers.Count(s => s.Status == "Active").ToString();

            InactiveSuppliersText.Text =
                suppliers.Count(s => s.Status == "Inactive").ToString();
        }
    }

    public class Supplier : INotifyPropertyChanged
    {
        private string companyName = "";
        private string contactPerson = "";
        private string phone = "";
        private string email = "";
        private string address = "";
        private string status = "Active";

        public int SupplierId { get; set; }

        public string CompanyName
        {
            get => companyName;
            set { companyName = value; OnPropertyChanged(nameof(CompanyName)); }
        }

        public string ContactPerson
        {
            get => contactPerson;
            set { contactPerson = value; OnPropertyChanged(nameof(ContactPerson)); }
        }

        public string Phone
        {
            get => phone;
            set { phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Address
        {
            get => address;
            set { address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(nameof(Status)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
