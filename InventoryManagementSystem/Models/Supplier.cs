
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = "";

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = "";

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = "";

        [MaxLength(300)]
        public string Address { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }
}
