using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
namespace PaymentService.Data
{
    public class PaymentServiceDbContext : DbContext
    {

        public PaymentServiceDbContext(DbContextOptions<PaymentServiceDbContext> options) : base(options)
        {

        }

        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<Payment> Payment { get; set; }

        public DbSet<PaymentMethod> PaymentMethod { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .ToTable("Orders");

            modelBuilder.Entity<OrderItem>()
                .ToTable("OrderItem");

            modelBuilder.Entity<Payment>()
                .ToTable("Payments");

            modelBuilder.Entity<PaymentMethod>()
                .ToTable("PaymentMethods");
        }
    }
}
