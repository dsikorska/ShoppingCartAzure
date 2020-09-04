using Microsoft.EntityFrameworkCore;
using ShoppingCart.DurableFunction.DataAccess.Models;
using ShoppingCart.DurableFunction.Shared.Models;
using System;

namespace ShoppingCart.DurableFunction
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartProduct> CartProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasKey(x => x.Id);

            modelBuilder.Entity<Cart>().HasKey(x => x.Id);
            modelBuilder.Entity<Cart>().Property(x => x.Status)
                .HasConversion(
                v => v.ToString(),
                v => (Status)Enum.Parse(typeof(Status), v));
            modelBuilder.Entity<Cart>().Property(x => x.Email).IsRequired().HasMaxLength(200);

            modelBuilder.Entity<CartProduct>().HasKey(x => x.Id);
            modelBuilder.Entity<CartProduct>().HasOne(x => x.Product).WithMany();
            modelBuilder.Entity<CartProduct>().HasOne(x => x.Cart).WithMany(x => x.Products);

            modelBuilder.Entity<Product>().HasData(
                new Product { Name = "A", Price = 1, Quantity = 100, Id = 1 },
                new Product { Name = "B", Price = 1.11M, Quantity = 50, Id = 2 },
                new Product { Name = "C", Price = 12.99M, Quantity = 25, Id = 3 }
                );
        }
    }
}