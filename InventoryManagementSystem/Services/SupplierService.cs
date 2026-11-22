
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class SupplierService
    {
        private readonly AppDbContextFactory factory = new();

        // =====================================
        // READ - GET ALL SUPPLIERS
        // =====================================

        public List<Supplier> GetAllSuppliers()
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            return db.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.CompanyName)
                .ToList();
        }

        // =====================================
        // CREATE - ADD SUPPLIER
        // =====================================

        public void AddSupplier(
            string companyName,
            string contactPerson,
            string phone,
            string email,
            string address,
            string status)
        {
            ValidateSupplier(
                companyName,
                contactPerson,
                phone,
                email,
                address,
                status);

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var supplier = new Supplier
            {
                CompanyName = companyName.Trim(),
                ContactPerson = contactPerson.Trim(),
                Phone = phone.Trim(),
                Email = email.Trim(),
                Address = address.Trim(),
                Status = status
            };

            db.Suppliers.Add(supplier);
            db.SaveChanges();
        }

        // =====================================
        // UPDATE - EDIT SUPPLIER
        // =====================================

        public void UpdateSupplier(
            int supplierId,
            string companyName,
            string contactPerson,
            string phone,
            string email,
            string address,
            string status)
        {
            ValidateSupplier(
                companyName,
                contactPerson,
                phone,
                email,
                address,
                status);

            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var supplier = db.Suppliers.Find(supplierId);

            if (supplier == null)
            {
                throw new InvalidOperationException(
                    "Supplier not found.");
            }

            supplier.CompanyName = companyName.Trim();
            supplier.ContactPerson = contactPerson.Trim();
            supplier.Phone = phone.Trim();
            supplier.Email = email.Trim();
            supplier.Address = address.Trim();
            supplier.Status = status;

            db.SaveChanges();
        }

        // =====================================
        // DELETE - REMOVE SUPPLIER
        // =====================================

        public void DeleteSupplier(int supplierId)
        {
            using var db = factory.CreateDbContext(
                Array.Empty<string>());

            var supplier = db.Suppliers.Find(supplierId);

            if (supplier == null)
            {
                throw new InvalidOperationException(
                    "Supplier not found.");
            }

            db.Suppliers.Remove(supplier);
            db.SaveChanges();
        }

        // =====================================
        // VALIDATION
        // =====================================

        private static void ValidateSupplier(
            string companyName,
            string contactPerson,
            string phone,
            string email,
            string address,
            string status)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new ArgumentException(
                    "Company name cannot be empty.");
            }

            if (companyName.Trim().Length > 150)
            {
                throw new ArgumentException(
                    "Company name cannot exceed 150 characters.");
            }

            if (string.IsNullOrWhiteSpace(contactPerson))
            {
                throw new ArgumentException(
                    "Contact person cannot be empty.");
            }

            if (contactPerson.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Contact person cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(phone) ||
                phone.Trim().Length > 30)
            {
                throw new ArgumentException(
                    "Please enter a valid phone number (maximum 30 characters).");
            }

            if (string.IsNullOrWhiteSpace(email) ||
                email.Trim().Length > 150 ||
                !System.Net.Mail.MailAddress.TryCreate(
                    email.Trim(),
                    out var parsedEmail) ||
                parsedEmail.Address != email.Trim())
            {
                throw new ArgumentException(
                    "Please enter a valid email address.");
            }

            if (address.Length > 300)
            {
                throw new ArgumentException(
                    "Address cannot exceed 300 characters.");
            }

            if (status != "Active" && status != "Inactive")
            {
                throw new ArgumentException(
                    "Status must be Active or Inactive.");
            }
        }
    }
}
