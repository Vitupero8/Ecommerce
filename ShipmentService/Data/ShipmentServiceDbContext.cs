using Microsoft.EntityFrameworkCore;
using ShipmentService.Models;

namespace ShipmentService.Data
{
    public class ShipmentServiceDbContext : DbContext
    {
        public ShipmentServiceDbContext(DbContextOptions<ShipmentServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Shipment> Shipments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shipment>()
                .HasKey(s => s.ShipmentID);

            modelBuilder.Entity<Shipment>()
                .ToTable("Shipments");
        }
    }
}