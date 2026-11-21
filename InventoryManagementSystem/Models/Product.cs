
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = "";

        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        // Foreign Key
        public int CategoryId { get; set; }

        // Each product belongs to one category.
        public Category Category { get; set; } = null!;

        [NotMapped]
        public string Status =>
            Quantity <= 10 ? "Low Stock" : "In Stock";
    }
}
