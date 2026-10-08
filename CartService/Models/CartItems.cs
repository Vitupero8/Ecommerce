namespace CartService.Models
{
    public class CartItems
    {
        public int CartItemID { get; set; }
        public int CartId { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
