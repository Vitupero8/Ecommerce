using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class CartItems
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

    }
}
