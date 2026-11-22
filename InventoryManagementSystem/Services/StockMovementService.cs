
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class StockMovementService
    {
        private readonly AppDbContextFactory factory = new();

        // ======================================
        // READ - GET ALL STOCK MOVEMENTS
        // ======================================

        public List<StockMovement> GetAllMovements()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            return db.StockMovements
                .Include(m => m.Product)
                .AsNoTracking()
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.MovementId)
                .ToList();
        }

        // ======================================
        // CREATE - STOCK IN / STOCK OUT
        // ======================================

        public void AddMovement(
            int productId,
            string movementType,
            int quantity,
            string note)
        {
            // Validate movement type
            if (movementType != "Stock In" &&
                movementType != "Stock Out")
            {
                throw new ArgumentException(
                    "Invalid movement type.");
            }

            // Validate quantity
            if (quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            if (note != null && note.Length > 500)
            {
                throw new ArgumentException(
                    "Note cannot exceed 500 characters.");
            }

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            // Keep stock update and movement record together
            using var transaction = db.Database.BeginTransaction();

            int affectedRows;

            if (movementType == "Stock In")
            {
                // Increase product stock
                affectedRows = db.Products
                    .Where(p => p.ProductId == productId)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(
                            p => p.Quantity,
                            p => p.Quantity + quantity));
            }
            else
            {
                // Decrease stock only when enough stock exists
                affectedRows = db.Products
                    .Where(p =>
                        p.ProductId == productId &&
                        p.Quantity >= quantity)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(
                            p => p.Quantity,
                            p => p.Quantity - quantity));
            }

            if (affectedRows == 0)
            {
                bool productExists = db.Products
                    .Any(p => p.ProductId == productId);

                if (!productExists)
                {
                    throw new InvalidOperationException(
                        "Product not found.");
                }

                throw new InvalidOperationException(
                    "Insufficient stock for this operation.");
            }

            // Save movement history
            db.StockMovements.Add(new StockMovement
            {
                ProductId = productId,
                MovementType = movementType,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Note = note?.Trim() ?? ""
            });

            db.SaveChanges();

            // Confirm both operations
            transaction.Commit();
        }
    }
}
