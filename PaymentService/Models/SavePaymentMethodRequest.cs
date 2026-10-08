using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class SavePaymentMethodRequest
    {
        public int CustomerID { get; set; }
        public string MethodType { get; set; }
        public string Identifier { get; set; }
    }
}
