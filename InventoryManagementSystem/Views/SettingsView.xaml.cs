
using System.Windows;
using System.Windows.Controls;

namespace InventoryManagementSystem.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text) ||
                string.IsNullOrWhiteSpace(EmailBox.Text) ||
                string.IsNullOrWhiteSpace(CompanyNameBox.Text))
            {
                MessageBox.Show(
                    "Please fill in all required fields.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            SaveStatusText.Text = "Changes saved successfully!";

            MessageBox.Show(
                "Your settings have been saved.",
                "NEXORA",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
