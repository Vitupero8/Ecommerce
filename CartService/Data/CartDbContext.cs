using Microsoft.EntityFrameworkCore;
using CartService.Models;

namespace CartService.Data
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
        {
        }

        public DbSet<Carts> Cart { get; set; }
        public DbSet<CartItems> CartItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Carts>()
                .HasKey(c => c.CartId);

            modelBuilder.Entity<Carts>()
                .ToTable("Carts");

            modelBuilder.Entity<CartItems>()
                .HasKey(c => c.CartItemID);

            modelBuilder.Entity<CartItems>()
                .ToTable("CartItems");
        }
    }
}