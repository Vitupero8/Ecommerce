using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PaymentService.Models;
namespace PaymentService.Services
{
    public class CardPaymentService
    {
        public bool ValidateCardNumber(string cardNumber)
        {
            int sum = 0;
            bool doubleNumber = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int number = cardNumber[i] - '0';

                if (doubleNumber)
                {
                    number *= 2;

                    if (number > 9)
                    {
                        number -= 9;
                    }
                }

                sum += number;
                doubleNumber = !doubleNumber;
            }

            return sum % 10 == 0;
        }

        public bool ValidateExpiryDate(string expiryDate)
        {
            if (!DateTime.TryParseExact(
                expiryDate,
                "MM/yy",
                null,
                System.Globalization.DateTimeStyles.None,
                out DateTime expiry))
            {
                return false;
            }

            DateTime currentDate = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                1);

            DateTime expiryDateLastMonth = expiry.AddMonths(1);

            return expiryDateLastMonth > currentDate;
        }

        public bool ValidateCVV(string cvv)
        {
            if (string.IsNullOrEmpty(cvv))
            {
                return false;
            }

            if (cvv.Length != 3 && cvv.Length != 4)
            {
                return false;
            }

            return cvv.All(char.IsDigit);
        }

        public bool ValidateCard(CardPaymentRequest request)
        {


            if (!ValidateCardNumber(request.CardNumber))
            {
                return false;
            }

            if (!ValidateExpiryDate(request.ExpiryDate))
            {
                return false;
            }

            if (!ValidateCVV(request.CVV))
            {
                return false;
            }

            return true;
        }

        public bool ShouldDeclinePayment(string cardNumber)
        {
            return cardNumber == "4000000000000002";
        }
    }
}
