namespace CartService.Models
{
    public class CheckoutRequest
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CheckoutItem> Items { get; set; }

    }
}
