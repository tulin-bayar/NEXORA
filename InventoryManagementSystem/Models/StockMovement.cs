
using System;

namespace InventoryManagementSystem.Models
{
    public class StockMovement
    {
        public int MovementId { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string MovementType { get; set; } = "";

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; }

        public string Note { get; set; } = "";
    }
}
