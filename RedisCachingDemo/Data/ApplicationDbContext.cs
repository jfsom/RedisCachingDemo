using Microsoft.EntityFrameworkCore;
using RedisCachingDemo.Models;
using System;

namespace RedisCachingDemo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Generate dummy data
           // var products = GenerateDummyProducts(1000);

            // Seed data into Products table
            //modelBuilder.Entity<Product>().HasData(products);
        }

        private static Product[] GenerateDummyProducts(int count)
        {
            var random = new Random();
            var products = new Product[count];

            for (int i = 1; i <= count; i++)
            {
                // Generate a random product name
                var name = $"Product {i}";

                // Randomly assign a category
                var category = random.Next(0, 3) switch
                {
                    0 => "Fruits",
                    1 => "Vegetables",
                    _ => "Beverages"
                };

                // Generate a random price between 10 and 1000
                var price = random.Next(10, 1001);

                // Generate a random quantity between 10 and 200
                var quantity = random.Next(10, 201);

                products[i - 1] = new Product
                {
                    Id = i, // Ensure unique Id for seed data
                    Name = name,
                    Category = category,
                    Price = price,
                    Quantity = quantity
                };
            }

            return products;
        }
    }
}