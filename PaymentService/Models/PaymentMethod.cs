using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class PaymentMethod
    {
        public int PaymentMethodID { get; set; }
        public int CustomerID { get; set; }
        public string MethodType { get; set; }
        public string Identifier { get; set; }
        public bool IsDefault { get; set; }

    }
}
