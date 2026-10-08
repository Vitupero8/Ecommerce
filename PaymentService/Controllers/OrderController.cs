using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;
        private readonly CartServiceClient _cartClient;

        private readonly PayPalService _payPalService;

        private readonly CardPaymentService _cardPaymentService;

        private readonly ApplePayService _applePayService;

        private readonly BankTransferService _bankTransferService;
        public OrderController(PaymentServiceDbContext context, CartServiceClient cartClient , PayPalService paypalservice, CardPaymentService cardPaymentService , ApplePayService applePayService , 
            BankTransferService bankTransferService)
        {
            _context = context;
            _cartClient = cartClient;
            _payPalService = paypalservice;
            _cardPaymentService = cardPaymentService;
            _applePayService = applePayService;
            _bankTransferService = bankTransferService;
        }


        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var Orders = _context.Order
                .Where(x => x.CustomerID == customerId)
                .ToList();

            return Ok(Orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var Order = _context.Order.Find(id);

            if (Order == null)
            {
                return NotFound();
            }

            if (Order.CustomerID != customerId)
            {
                return Forbid();
            }

            return Ok(Order);
        }

        [HttpPost]
        public IActionResult CreateOrder(CheckoutRequest request)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var order = new Order
            {
                CartID = request.CartId,
                CustomerID = customerId,
                TotalPrice = request.TotalPrice,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Order.Add(order);
            _context.SaveChanges();

            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderID = order.OrderId,
                    ProductID = item.ProductId,
                    Price = item.Price,
                    Quantity = item.Quantity
                };

                _context.OrderItem.Add(orderItem);
            }

            _context.SaveChanges();

            return Ok(order);
        }

        [HttpPost("{id}/payment")]
        public async Task<IActionResult> PayOrder(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var paypalOrder = await _payPalService.CreatePayPalOrder(order.TotalPrice);

            using var json = JsonDocument.Parse(paypalOrder);

            var paypalOrderID = json.RootElement
                .GetProperty("id")
                .GetString();

            order.PayPalOrderID = paypalOrderID;

            _context.SaveChanges();

            return Ok(paypalOrder);
        }

        [HttpGet("paypal-test")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalTest(
        [FromServices] PayPalService payPayService)
        {
            var token = await payPayService.GetAccessToken();

            return Ok("Paypal connection successful!");
        }

        [HttpGet("paypal-success")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalSuccess(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("PayPal order ID is missing.");
            }

            var result = await _payPalService.CapturePayPalOrder(token);

            using var json = JsonDocument.Parse(result);

            var status = json.RootElement
                .GetProperty("status")
                .GetString();

            if (status != "COMPLETED")
            {
                return BadRequest("PayPal payment was not completed.");
            }

            var captureId = json.RootElement
                .GetProperty("purchase_units")[0]
                .GetProperty("payments")
                .GetProperty("captures")[0]
                .GetProperty("id")
                .GetString();

            var order = _context.Order
    .FirstOrDefault(x => x.PayPalOrderID == token);

            if (order == null)
            {
                return NotFound("Order does not exist.");
            }

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Completed",
                PaymentMethod = "PayPal",
                ExternalPaymentID = captureId,
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("{id}/card-payment")]
        public IActionResult PayWithCard(int id, CardPaymentRequest request)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }


            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            if (!_cardPaymentService.ValidateCard(request))
            {
                return BadRequest("Card details are invalid");
            }

            if (_cardPaymentService.ShouldDeclinePayment(request.CardNumber))
            {
                var failedPayment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Failed",
                    PaymentMethod = "Card",
                    ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(failedPayment);
                _context.SaveChanges();

                return BadRequest(failedPayment);
            }

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Completed",
                PaymentMethod = "Card",
                ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("{id}/apple-pay")]
        public IActionResult PayWithApplePay(int id, ApplePayPaymentRequest request)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            if (!_applePayService.ValidateApplePayToken(request.Token))
            {
                return BadRequest("Apple Pay token is invalid");
            }

            if (_applePayService.ShouldDeclinePayment(request.Token))
            {
                var failedPayment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Failed",
                    PaymentMethod = "Apple Pay",
                    ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(failedPayment);
                _context.SaveChanges();

                return BadRequest(failedPayment);
            }

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Completed",
                PaymentMethod = "Apple Pay",
                ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("{id}/bank-transfer")]
        public IActionResult PayWithBankTransfer(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var reference = _bankTransferService.GenerateReference();

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Pending",
                PaymentMethod = "Bank Transfer",
                ExternalPaymentID = reference,
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("bank-transfer/{paymentId}/confirm")]
        public IActionResult ConfirmBankTransfer(int paymentId)
        {
            var payment = _context.Payment.Find(paymentId);

            if (payment == null)
            {
                return NotFound("Payment does not exist!");
            }

            if (payment.PaymentMethod != "Bank Transfer")
            {
                return BadRequest("This is not a bank transfer");
            }

            if (payment.Status != "Pending")
            {
                return BadRequest("This payment cannot be confirmed");
            }

            var order = _context.Order.Find(payment.OrderID);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            payment.Status = "Completed";
            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("{id}/saved-card-payment")]
        public IActionResult PayWithSavedCard(int id, SavedPaymentMethodRequest request)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var paymentMethod = _context.PaymentMethod.Find(request.PaymentMethodID);

            if (paymentMethod == null)
            {
                return NotFound("Payment method does not exist!");
            }

            if (paymentMethod.MethodType != "Card")
            {
                return BadRequest("This payment method is not a Card");
            }

            if (paymentMethod.CustomerID != order.CustomerID)
            {
                return BadRequest("This payment method does not belong to the customer");
            }
            if (paymentMethod.Identifier == "CARD-FAIL")
            {
                var failedPayment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Failed",
                    PaymentMethod = "Card",
                    ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(failedPayment);
                _context.SaveChanges();

                return BadRequest(failedPayment);
            }

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Completed",
                PaymentMethod = "Card",
                ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }

        [HttpPost("{id}/saved-paypal-payment")]
        public async Task<IActionResult> PayWithSavedPayPal(
    int id,
    SavedPaymentMethodRequest request)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var paymentMethod = _context.PaymentMethod.Find(request.PaymentMethodID);

            if (paymentMethod == null)
            {
                return NotFound("Payment method does not exist!");
            }

            if (paymentMethod.MethodType != "PayPal")
            {
                return BadRequest("This payment method is not PayPal");
            }

            if (paymentMethod.CustomerID != order.CustomerID)
            {
                return BadRequest("This payment method does not belong to the customer");
            }

            var paypalOrder = await _payPalService.CreatePayPalOrder(order.TotalPrice);

            using var json = JsonDocument.Parse(paypalOrder);

            var paypalOrderID = json.RootElement
                .GetProperty("id")
                .GetString();

            order.PayPalOrderID = paypalOrderID;

            _context.SaveChanges();

            return Ok(paypalOrder);
        }

        [HttpPost("{id}/saved-apple-pay")]
        public IActionResult PayWithSavedApplePay(
    int id,
    SavedPaymentMethodRequest request)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var paymentMethod = _context.PaymentMethod.Find(request.PaymentMethodID);

            if (paymentMethod == null)
            {
                return NotFound("Payment method does not exist!");
            }

            if (paymentMethod.MethodType != "Apple Pay")
            {
                return BadRequest("This payment method is not Apple Pay");
            }

            if (paymentMethod.CustomerID != order.CustomerID)
            {
                return BadRequest("This payment method does not belong to the customer");
            }

            if (paymentMethod.Identifier == "APPLE-PAY-FAIL")
            {
                var failedPayment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Failed",
                    PaymentMethod = "Apple Pay",
                    ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(failedPayment);
                _context.SaveChanges();

                return BadRequest(failedPayment);
            }

            var payment = new Payment
            {
                OrderID = order.OrderId,
                Amount = order.TotalPrice,
                Status = "Completed",
                PaymentMethod = "Apple Pay",
                ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now
            };

            _context.Payment.Add(payment);

            order.Status = "Paid";

            _context.SaveChanges();

            return Ok(payment);
        }
        [HttpPost("{id}/default-payment")]
        public async Task<IActionResult> PayWithDefaultPayment(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound("Order does not exist!");
            }

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("This order cannot be paid");
            }

            if (order.TotalPrice <= 0)
            {
                return BadRequest("Order could not be paid");
            }

            var paymentMethod = _context.PaymentMethod
                .FirstOrDefault(x => x.CustomerID == order.CustomerID && x.IsDefault);

            if (paymentMethod == null)
            {
                return NotFound("Customer does not have a default payment method!");
            }

            if (paymentMethod.MethodType == "Card")
            {
                if (paymentMethod.Identifier == "CARD-FAIL")
                {
                    var failedPayment = new Payment
                    {
                        OrderID = order.OrderId,
                        Amount = order.TotalPrice,
                        Status = "Failed",
                        PaymentMethod = "Card",
                        ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now
                    };

                    _context.Payment.Add(failedPayment);
                    _context.SaveChanges();

                    return BadRequest(failedPayment);
                }

                var payment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Completed",
                    PaymentMethod = "Card",
                    ExternalPaymentID = "CARD-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(payment);

                order.Status = "Paid";

                _context.SaveChanges();

                return Ok(payment);
            }

            if (paymentMethod.MethodType == "Apple Pay")
            {
                if (paymentMethod.Identifier == "APPLE-PAY-FAIL")
                {
                    var failedPayment = new Payment
                    {
                        OrderID = order.OrderId,
                        Amount = order.TotalPrice,
                        Status = "Failed",
                        PaymentMethod = "Apple Pay",
                        ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now
                    };

                    _context.Payment.Add(failedPayment);
                    _context.SaveChanges();

                    return BadRequest(failedPayment);
                }

                var payment = new Payment
                {
                    OrderID = order.OrderId,
                    Amount = order.TotalPrice,
                    Status = "Completed",
                    PaymentMethod = "Apple Pay",
                    ExternalPaymentID = "APPLEPAY-" + Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.Now
                };

                _context.Payment.Add(payment);

                order.Status = "Paid";

                _context.SaveChanges();

                return Ok(payment);
            }

            if (paymentMethod.MethodType == "PayPal")
            {
                var paypalOrder = await _payPalService.CreatePayPalOrder(order.TotalPrice);

                using var json = JsonDocument.Parse(paypalOrder);

                var paypalOrderID = json.RootElement
                    .GetProperty("id")
                    .GetString();

                order.PayPalOrderID = paypalOrderID;

                _context.SaveChanges();

                return Ok(paypalOrder);
            }

            return BadRequest("Unsupported payment method");
        }






    }
    
}
