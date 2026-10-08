using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
