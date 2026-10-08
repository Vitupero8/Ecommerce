using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class CardPaymentRequest
    {
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }

    }
}
