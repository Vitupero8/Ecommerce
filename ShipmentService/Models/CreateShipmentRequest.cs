namespace ShipmentService.Models
{
    public class CreateShipmentRequest
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
    }
}
