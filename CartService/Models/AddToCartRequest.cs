namespace CartService.Models
{
    public class AddToCartRequest
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
