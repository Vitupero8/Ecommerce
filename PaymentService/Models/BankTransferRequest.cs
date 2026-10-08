using Microsoft.EntityFrameworkCore;
namespace PaymentService.Models
{
    public class BankTransferRequest
    {
        public int CustomerId { get; set; }
    }
}
