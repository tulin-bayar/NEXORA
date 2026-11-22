
using System;
using System.Linq;
using System.Net.Mail;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class ApplicationSettingService
    {
        private readonly AppDbContextFactory factory = new();

        // READ - Load saved settings
        public ApplicationSetting GetSettings()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var settings = db.ApplicationSettings
                .AsNoTracking()
                .OrderBy(s => s.ApplicationSettingId)
                .FirstOrDefault();

            // Use defaults if settings have never been saved
            return settings ?? new ApplicationSetting();
        }

        // CREATE / UPDATE - Save settings
        public void SaveSettings(ApplicationSetting newSettings)
        {
            ValidateSettings(newSettings);

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var settings = db.ApplicationSettings
                .OrderBy(s => s.ApplicationSettingId)
                .FirstOrDefault();

            if (settings == null)
            {
                settings = new ApplicationSetting();
                db.ApplicationSettings.Add(settings);
            }

            settings.FullName = newSettings.FullName.Trim();
            settings.Email = newSettings.Email.Trim();
            settings.CompanyName = newSettings.CompanyName.Trim();

            settings.Currency = newSettings.Currency;
            settings.Language = newSettings.Language;

            settings.LowStockNotification =
                newSettings.LowStockNotification;

            settings.MovementNotification =
                newSettings.MovementNotification;

            settings.SupplierNotification =
                newSettings.SupplierNotification;

            settings.Theme = newSettings.Theme;

            db.SaveChanges();
        }

        // VALIDATION
        private static void ValidateSettings(
            ApplicationSetting settings)
        {
            if (string.IsNullOrWhiteSpace(settings.FullName) ||
                settings.FullName.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Full name is required (maximum 100 characters).");
            }

            string email = settings.Email?.Trim() ?? "";

            if (email.Length == 0 ||
                email.Length > 150 ||
                !MailAddress.TryCreate(email, out var parsedEmail) ||
                !string.Equals(
                    parsedEmail.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Please enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(settings.CompanyName) ||
                settings.CompanyName.Trim().Length > 150)
            {
                throw new ArgumentException(
                    "Company name is required (maximum 150 characters).");
            }

            string[] currencies =
            {
                "USD ($)",
                "EUR (€)",
                "HUF (Ft)",
                "AZN (₼)",
                "TRY (₺)"
            };

            if (!currencies.Contains(settings.Currency))
            {
                throw new ArgumentException(
                    "Please select a valid currency.");
            }

            string[] languages =
            {
                "English",
                "Azerbaijani",
                "Hungarian",
                "Turkish"
            };

            if (!languages.Contains(settings.Language))
            {
                throw new ArgumentException(
                    "Please select a valid language.");
            }

            if (settings.Theme != "Light Mode")
            {
                throw new ArgumentException(
                    "Dark Mode is not available yet. Please select Light Mode.");
            }
        }
    }
}
