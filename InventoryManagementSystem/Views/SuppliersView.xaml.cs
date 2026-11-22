
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.Views
{
    public partial class SuppliersView : UserControl
    {
        private readonly ObservableCollection<Supplier> suppliers = new();

        private ICollectionView? suppliersView;

        private Supplier? selectedSupplier;

        private readonly SupplierService supplierService = new();

        public SuppliersView()
        {
            InitializeComponent();

            suppliersView =
                CollectionViewSource.GetDefaultView(suppliers);

            suppliersView.Filter = FilterSuppliers;

            SuppliersDataGrid.ItemsSource = suppliersView;

            ClearForm();

            try
            {
                LoadSuppliers();
            }
            catch (Exception ex)
            {
                ShowMessage("Database Error", ex.Message);
            }

            // Refresh when returning to Suppliers page
            IsVisibleChanged += (sender, e) =>
            {
                if (e.NewValue is bool visible && visible)
                {
                    try
                    {
                        LoadSuppliers();
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Database Error", ex.Message);
                    }
                }
            };
        }

        // =====================================
        // READ - LOAD SUPPLIERS FROM SQL SERVER
        // =====================================

        private void LoadSuppliers()
        {
            var databaseSuppliers =
                supplierService.GetAllSuppliers();

            suppliers.Clear();

            foreach (var supplier in databaseSuppliers)
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = supplier.SupplierId,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    Status = supplier.Status
                });
            }

            suppliersView?.Refresh();

            UpdateStatistics();
        }

        // =====================================
        // SEARCH AND STATUS FILTER
        // =====================================

        private bool FilterSuppliers(object item)
        {
            if (item is not Supplier supplier)
                return false;

            string searchText =
                SearchBox?.Text?.Trim() ?? "";

            bool matchesSearch =
                supplier.CompanyName.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) ||

                supplier.ContactPerson.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) ||

                supplier.Email.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) ||

                supplier.Phone.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase);

            string selectedStatus =
                (StatusFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "All Suppliers";

            bool matchesStatus =
                selectedStatus == "All Suppliers" ||
                supplier.Status == selectedStatus;

            return matchesSearch && matchesStatus;
        }

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            suppliersView?.Refresh();
        }

        private void StatusFilter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            suppliersView?.Refresh();
        }

        // =====================================
        // ADD SUPPLIER BUTTON
        // =====================================

        private void AddSupplierButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ClearForm();
            CompanyNameBox.Focus();
        }

        // =====================================
        // CREATE / UPDATE SUPPLIER
        // =====================================

        private void SaveSupplierButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string companyName =
                CompanyNameBox.Text.Trim();

            string contactPerson =
                ContactPersonBox.Text.Trim();

            string phone =
                PhoneBox.Text.Trim();

            string email =
                EmailBox.Text.Trim();

            string address =
                AddressBox.Text.Trim();

            string status =
                (StatusBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Active";

            // BASIC VALIDATION

            if (string.IsNullOrWhiteSpace(companyName))
            {
                ShowMessage(
                    "Validation",
                    "Please enter the company name.");

                CompanyNameBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contactPerson))
            {
                ShowMessage(
                    "Validation",
                    "Please enter the contact person.");

                ContactPersonBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowMessage(
                    "Validation",
                    "Please enter the phone number.");

                PhoneBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowMessage(
                    "Validation",
                    "Please enter the email address.");

                EmailBox.Focus();
                return;
            }

            try
            {
                if (selectedSupplier == null)
                {
                    // CREATE - SQL SERVER

                    supplierService.AddSupplier(
                        companyName,
                        contactPerson,
                        phone,
                        email,
                        address,
                        status);

                    LoadSuppliers();
                    ClearForm();

                    ShowMessage(
                        "Success",
                        "Supplier added successfully!");
                }
                else
                {
                    // UPDATE - SQL SERVER

                    supplierService.UpdateSupplier(
                        selectedSupplier.SupplierId,
                        companyName,
                        contactPerson,
                        phone,
                        email,
                        address,
                        status);

                    LoadSuppliers();
                    ClearForm();

                    ShowMessage(
                        "Success",
                        "Supplier updated successfully!");
                }
            }
            catch (ArgumentException ex)
            {
                ShowMessage(
                    "Validation Error",
                    ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error saving supplier: {ex.Message}");
            }
        }

        // =====================================
        // EDIT SUPPLIER
        // =====================================

        private void EditSupplierButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Supplier supplier)
            {
                return;
            }

            selectedSupplier = supplier;

            CompanyNameBox.Text =
                supplier.CompanyName;

            ContactPersonBox.Text =
                supplier.ContactPerson;

            PhoneBox.Text =
                supplier.Phone;

            EmailBox.Text =
                supplier.Email;

            AddressBox.Text =
                supplier.Address;

            StatusBox.SelectedIndex =
                supplier.Status == "Inactive" ? 1 : 0;

            FormTitleText.Text =
                "Edit Supplier Information";

            SaveSupplierButton.Content =
                "Update Supplier";

            CancelEditButton.Visibility =
                Visibility.Visible;
        }

        // =====================================
        // DELETE SUPPLIER
        // =====================================

        private void DeleteSupplierButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Supplier supplier)
            {
                return;
            }

            bool confirmed = EnglishDialog.Confirm(
                Window.GetWindow(this),
                "Delete Supplier",
                $"Are you sure you want to delete '{supplier.CompanyName}'?");

            if (!confirmed)
                return;

            try
            {
                // DELETE FROM SQL SERVER

                supplierService.DeleteSupplier(
                    supplier.SupplierId);

                if (selectedSupplier?.SupplierId ==
                    supplier.SupplierId)
                {
                    ClearForm();
                }

                LoadSuppliers();

                ShowMessage(
                    "Success",
                    "Supplier deleted successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    $"Error deleting supplier: {ex.Message}");
            }
        }

        // =====================================
        // CANCEL EDIT
        // =====================================

        private void CancelEditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ClearForm();
        }

        // =====================================
        // CLEAR FORM
        // =====================================

        private void ClearForm()
        {
            selectedSupplier = null;

            CompanyNameBox.Clear();
            ContactPersonBox.Clear();
            PhoneBox.Clear();
            EmailBox.Clear();
            AddressBox.Clear();

            StatusBox.SelectedIndex = 0;

            FormTitleText.Text =
                "Supplier Information";

            SaveSupplierButton.Content =
                "Save Supplier";

            CancelEditButton.Visibility =
                Visibility.Collapsed;
        }

        // =====================================
        // UPDATE STATISTICS
        // =====================================

        private void UpdateStatistics()
        {
            TotalSuppliersText.Text =
                suppliers.Count.ToString();

            ActiveSuppliersText.Text =
                suppliers.Count(s =>
                    s.Status == "Active").ToString();

            InactiveSuppliersText.Text =
                suppliers.Count(s =>
                    s.Status == "Inactive").ToString();
        }

        // =====================================
        // ENGLISH MESSAGE DIALOG
        // =====================================

        private void ShowMessage(
            string title,
            string message)
        {
            EnglishDialog.Alert(
                Window.GetWindow(this),
                title,
                message);
        }
    }

    // =====================================
    // UI MODEL - SUPPLIER
    // =====================================

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
            set
            {
                companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        public string ContactPerson
        {
            get => contactPerson;
            set
            {
                contactPerson = value;
                OnPropertyChanged(nameof(ContactPerson));
            }
        }

        public string Phone
        {
            get => phone;
            set
            {
                phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Address
        {
            get => address;
            set
            {
                address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler?
            PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
