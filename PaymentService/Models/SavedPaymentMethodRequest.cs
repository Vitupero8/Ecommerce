using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class SavedPaymentMethodRequest
    {
        public int PaymentMethodID { get; set; }
    }
}
