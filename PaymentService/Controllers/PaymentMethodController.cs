using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Models;


namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public PaymentMethodController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SavePaymentMethod(SavePaymentMethodRequest request)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var existingMethods = _context.PaymentMethod
                .Where(x => x.CustomerID == customerId)
                .ToList();

            bool isDefault = existingMethods.Count == 0;

            var paymentMethod = new PaymentMethod
            {
                CustomerID = customerId,
                MethodType = request.MethodType,
                Identifier = request.Identifier,
                IsDefault = isDefault
            };

            _context.PaymentMethod.Add(paymentMethod);
            _context.SaveChanges();

            return Ok(paymentMethod);
        }

        [HttpPut("{id}/default")]
        public IActionResult SetDefaultPaymentMethod(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var paymentMethod = _context.PaymentMethod.Find(id);

            if (paymentMethod == null)
            {
                return NotFound("Payment method does not exist!");
            }

            if (paymentMethod.CustomerID != customerId)
            {
                return Forbid();
            }

            var customerMethods = _context.PaymentMethod
                .Where(x => x.CustomerID == customerId)
                .ToList();

            foreach (var method in customerMethods)
            {
                method.IsDefault = false;
            }

            paymentMethod.IsDefault = true;

            _context.SaveChanges();

            return Ok(paymentMethod);
        }

        [HttpGet("customer/{customerId}")]
        public IActionResult GetCustomerPaymentMethods(int customerId)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int loggedInCustomerId = int.Parse(customerIdClaim.Value);

            if (customerId != loggedInCustomerId)
            {
                return Forbid();
            }

            var paymentMethods = _context.PaymentMethod
                .Where(x => x.CustomerID == loggedInCustomerId)
                .ToList();

            return Ok(paymentMethods);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePaymentMethod(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var paymentMethod = _context.PaymentMethod.Find(id);

            if (paymentMethod == null)
            {
                return NotFound("Payment method does not exist!");
            }

            if (paymentMethod.CustomerID != customerId)
            {
                return Forbid();
            }

            bool wasDefault = paymentMethod.IsDefault;

            _context.PaymentMethod.Remove(paymentMethod);
            _context.SaveChanges();

            if (wasDefault)
            {
                var nextMethod = _context.PaymentMethod
                    .FirstOrDefault(x => x.CustomerID == customerId);

                if (nextMethod != null)
                {
                    nextMethod.IsDefault = true;
                    _context.SaveChanges();
                }
            }

            return Ok("Payment method deleted");
        }
    }
}