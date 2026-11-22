
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementSystem.Dialogs;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly ApplicationSettingService
            settingService = new();

        public SettingsView()
        {
            InitializeComponent();

            // Load saved settings when the page becomes visible
            IsVisibleChanged += (sender, e) =>
            {
                if (e.NewValue is bool visible && visible)
                {
                    LoadSettings();
                }
            };
        }

        // =====================================
        // READ - LOAD SETTINGS FROM SQL SERVER
        // =====================================

        private void LoadSettings()
        {
            try
            {
                ApplicationSetting settings =
                    settingService.GetSettings();

                // PROFILE
                FullNameBox.Text = settings.FullName;
                EmailBox.Text = settings.Email;

                // GENERAL
                CompanyNameBox.Text = settings.CompanyName;

                SelectComboBoxItem(
                    CurrencyBox,
                    settings.Currency);

                SelectComboBoxItem(
                    LanguageBox,
                    settings.Language);

                // NOTIFICATIONS
                LowStockNotification.IsChecked =
                    settings.LowStockNotification;

                MovementNotification.IsChecked =
                    settings.MovementNotification;

                SupplierNotification.IsChecked =
                    settings.SupplierNotification;

                // APPEARANCE
                SelectComboBoxItem(
                    ThemeBox,
                    settings.Theme);

                SaveStatusText.Text = "";
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Database Error",
                    "Could not load settings:\n" + ex.Message);
            }
        }

        // =====================================
        // SAVE CHANGES BUTTON
        // =====================================

        private void SaveChangesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SaveStatusText.Text = "";

            try
            {
                ApplicationSetting settings =
                    new ApplicationSetting
                    {
                        // PROFILE
                        FullName = FullNameBox.Text.Trim(),

                        Email = EmailBox.Text.Trim(),

                        // GENERAL
                        CompanyName =
                            CompanyNameBox.Text.Trim(),

                        Currency =
                            GetComboBoxValue(CurrencyBox),

                        Language =
                            GetComboBoxValue(LanguageBox),

                        // NOTIFICATIONS
                        LowStockNotification =
                            LowStockNotification.IsChecked == true,

                        MovementNotification =
                            MovementNotification.IsChecked == true,

                        SupplierNotification =
                            SupplierNotification.IsChecked == true,

                        // APPEARANCE
                        Theme =
                            GetComboBoxValue(ThemeBox)
                    };

                // SAVE TO SQL SERVER
                settingService.SaveSettings(settings);

                SaveStatusText.Text =
                    "Changes saved successfully!";

                ShowMessage(
                    "Success",
                    "Your settings have been saved successfully!");
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
                    "Could not save settings:\n" + ex.Message);
            }
        }

        // =====================================
        // COMBOBOX HELPERS
        // =====================================

        private static string GetComboBoxValue(
            ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? "";
            }

            return "";
        }

        private static void SelectComboBoxItem(
            ComboBox comboBox,
            string value)
        {
            foreach (ComboBoxItem item in
                comboBox.Items.OfType<ComboBoxItem>())
            {
                if (string.Equals(
                    item.Content?.ToString(),
                    value,
                    StringComparison.Ordinal))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = 0;
        }

        // =====================================
        // ENGLISH MESSAGE WINDOW
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
}
