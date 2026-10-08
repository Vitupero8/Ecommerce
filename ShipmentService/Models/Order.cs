
namespace ShipmentService.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
