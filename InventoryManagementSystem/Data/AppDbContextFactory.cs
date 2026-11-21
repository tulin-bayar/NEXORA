
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryManagementSystem.Data
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Find the configuration file
            string[] possiblePaths =
            {
                Path.Combine(Directory.GetCurrentDirectory(),
                    "appsettings.json"),

                Path.Combine(Directory.GetCurrentDirectory(),
                    "InventoryManagementSystem",
                    "appsettings.json"),

                Path.Combine(AppContext.BaseDirectory,
                    "appsettings.json")
            };

            string configPath = possiblePaths
                .FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException(
                    "appsettings.json was not found.");

            // Read the SQL Server connection string
            using var document = JsonDocument.Parse(
                File.ReadAllText(configPath));

            string connectionString = document.RootElement
                .GetProperty("ConnectionStrings")
                .GetProperty("DefaultConnection")
                .GetString()
                ?? throw new InvalidOperationException(
                    "DefaultConnection is missing.");

            // Configure Entity Framework Core
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}
