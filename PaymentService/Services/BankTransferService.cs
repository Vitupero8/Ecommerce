using Microsoft.EntityFrameworkCore;
namespace PaymentService.Services
{
    public class BankTransferService
    {
        public string GenerateReference()
        {
            return "BANK-" + Guid.NewGuid().ToString("N");
        }
    }
}
