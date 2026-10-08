using Microsoft.EntityFrameworkCore;
namespace ShipmentService.Models
{
    public class Shipment
    {
        public int ShipmentID { get; set; }
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
