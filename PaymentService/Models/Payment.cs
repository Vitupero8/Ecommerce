using Microsoft.EntityFrameworkCore;  
namespace PaymentService.Models
{
 public class Payment
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string ExternalPaymentID { get; set; }
        
     
        public DateTime CreatedAt { get; set; }

    }
}
