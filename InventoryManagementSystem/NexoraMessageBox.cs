
global using MessageBox = InventoryManagementSystem.Dialogs.NexoraMessageBox;

using System;
using System.Windows;

namespace InventoryManagementSystem.Dialogs
{
    public static class NexoraMessageBox
    {
        // MessageBox.Show("Message")
        public static MessageBoxResult Show(string message)
        {
            return Show(
                message,
                "NEXORA",
                MessageBoxButton.OK,
                MessageBoxImage.None);
        }

        // MessageBox.Show("Message", "Title")
        public static MessageBoxResult Show(
            string message,
            string title)
        {
            return Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.None);
        }

        // MessageBox.Show("Message", "Title", buttons)
        public static MessageBoxResult Show(
            string message,
            string title,
            MessageBoxButton buttons)
        {
            return Show(
                message,
                title,
                buttons,
                MessageBoxImage.None);
        }

        // MessageBox.Show("Message", "Title", buttons, icon)
        public static MessageBoxResult Show(
            string message,
            string title,
            MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            Window? owner = Application.Current?.MainWindow;

            if (buttons == MessageBoxButton.YesNo)
            {
                bool confirmed = EnglishDialog.Confirm(
                    owner,
                    title,
                    message);

                return confirmed
                    ? MessageBoxResult.Yes
                    : MessageBoxResult.No;
            }

            if (buttons == MessageBoxButton.OK)
            {
                EnglishDialog.Alert(
                    owner,
                    title,
                    message);

                return MessageBoxResult.OK;
            }

            throw new NotSupportedException(
                "This dialog button combination is not supported.");
        }
    }
}
