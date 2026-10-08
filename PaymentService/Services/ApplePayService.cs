using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PaymentService.Models;
namespace PaymentService.Services
{
    public class ApplePayService
    {
        public bool ValidateApplePayToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!token.StartsWith("APPLE-PAY-"))
            {
                return false;
            }

            return true;
        }

        public bool ShouldDeclinePayment(string token)
        {
            return token == "APPLE-PAY-FAIL";
        }
    }


}
