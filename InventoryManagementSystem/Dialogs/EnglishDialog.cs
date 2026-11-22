
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventoryManagementSystem.Dialogs
{
    public static class EnglishDialog
    {
        public static bool Confirm(
            Window? owner, string title, string message)
        {
            return Show(owner, title, message, true);
        }

        public static void Alert(
            Window? owner, string title, string message)
        {
            Show(owner, title, message, false);
        }

        private static bool Show(
            Window? owner, string title,
            string message, bool confirmation)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 440,
                MinHeight = 180,
                MaxHeight = 400,
                SizeToContent = SizeToContent.Height,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = Brushes.White,
                WindowStartupLocation = owner == null
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner
            };

            if (owner != null)
                dialog.Owner = owner;

            var layout = new StackPanel
            {
                Margin = new Thickness(24)
            };

            layout.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 28)
            });

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            if (confirmation)
            {
                var noButton = new Button
                {
                    Content = "No",
                    Width = 90,
                    Height = 36,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsCancel = true
                };

                noButton.Click += (s, e) =>
                    dialog.DialogResult = false;

                buttons.Children.Add(noButton);
            }

            var yesButton = new Button
            {
                Content = confirmation ? "Yes" : "OK",
                Width = 90,
                Height = 36,
                Background = new SolidColorBrush(
                    Color.FromRgb(49, 91, 134)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                IsDefault = true
            };

            yesButton.Click += (s, e) =>
                dialog.DialogResult = true;

            buttons.Children.Add(yesButton);
            layout.Children.Add(buttons);

            dialog.Content = layout;

            return dialog.ShowDialog() == true;
        }
    }
}
