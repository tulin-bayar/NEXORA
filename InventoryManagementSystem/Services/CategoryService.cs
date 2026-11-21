
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class CategoryService
    {
        private readonly AppDbContextFactory factory = new();

        // READ - List categories

        public List<Category> GetAllCategories()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            return db.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToList();
        }


        // CREATE - Add category
        public void AddCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Category name cannot be empty.");

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            db.Categories.Add(new Category
            {
                Name = name.Trim(),
                Description = description.Trim()
            });

            db.SaveChanges();
        }

        // UPDATE - Edit category
        public void UpdateCategory(
            int id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Category name cannot be empty.");

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var category = db.Categories.Find(id);

            if (category == null)
                throw new InvalidOperationException(
                    "Category not found.");

            category.Name = name.Trim();
            category.Description = description.Trim();

            db.SaveChanges();
        }

        // DELETE - Remove category
        public void DeleteCategory(int id)
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var category = db.Categories.Find(id);

            if (category == null)
                return;

            db.Categories.Remove(category);
            db.SaveChanges();
        }
    }
}
