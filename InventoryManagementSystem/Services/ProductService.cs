
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class ProductService
    {
        private readonly AppDbContextFactory factory = new();

        // READ - Get all products
        public List<Product> GetAllProducts()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            return db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToList();
        }

        // CREATE - Add a new product
        public void AddProduct(
            string name,
            int categoryId,
            int quantity,
            decimal price)
        {
            ValidateProduct(name, quantity, price);

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            if (!db.Categories.Any(c =>
                c.CategoryId == categoryId))
            {
                throw new ArgumentException(
                    "Selected category does not exist.");
            }

            db.Products.Add(new Product
            {
                Name = name.Trim(),
                CategoryId = categoryId,
                Quantity = quantity,
                Price = price
            });

            db.SaveChanges();
        }

        // UPDATE - Edit product
        public void UpdateProduct(
            int id,
            string name,
            int categoryId,
            int quantity,
            decimal price)
        {
            ValidateProduct(name, quantity, price);

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var product = db.Products.Find(id);

            if (product == null)
                throw new InvalidOperationException(
                    "Product not found.");

            if (!db.Categories.Any(c =>
                c.CategoryId == categoryId))
            {
                throw new ArgumentException(
                    "Selected category does not exist.");
            }

            product.Name = name.Trim();
            product.CategoryId = categoryId;
            product.Quantity = quantity;
            product.Price = price;

            db.SaveChanges();
        }

        // DELETE - Remove product
        public void DeleteProduct(int id)
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var product = db.Products.Find(id);

            if (product == null)
                return;

            db.Products.Remove(product);
            db.SaveChanges();
        }

        // VALIDATION
        private static void ValidateProduct(
            string name,
            int quantity,
            decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Product name cannot be empty.");

            if (quantity < 0)
                throw new ArgumentException(
                    "Quantity cannot be negative.");

            if (price < 0)
                throw new ArgumentException(
                    "Price cannot be negative.");
        }
    }
}
