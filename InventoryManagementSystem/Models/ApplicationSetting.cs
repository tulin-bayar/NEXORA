
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class ApplicationSetting
    {
        public int ApplicationSettingId { get; set; }

        // PROFILE SETTINGS

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "Administrator";

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = "admin@nexora.com";

        // GENERAL SETTINGS

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = "NEXORA";

        [Required]
        [MaxLength(20)]
        public string Currency { get; set; } = "USD ($)";

        [Required]
        [MaxLength(30)]
        public string Language { get; set; } = "English";

        // NOTIFICATION PREFERENCES

        public bool LowStockNotification { get; set; } = true;

        public bool MovementNotification { get; set; } = true;

        public bool SupplierNotification { get; set; } = false;

        // APPEARANCE

        [Required]
        [MaxLength(50)]
        public string Theme { get; set; } = "Light Mode";
    }
}
