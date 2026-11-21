
using InventoryManagementSystem.Views;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        // One category can contain many products.
        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}
