using Microsoft.EntityFrameworkCore;
using ShipmentService.Models;
using ShipmentService.Data;
using Microsoft.AspNetCore.Mvc;
using ShipmentService.Services;
using Microsoft.AspNetCore.Authorization;
namespace ShipmentService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly ShipmentServiceDbContext _context;
        private readonly PaymemtServiceClient _paymentServiceClient;

        private readonly CustomerServiceClient _customerServiceClient;

        public ShipmentController(ShipmentServiceDbContext context , PaymemtServiceClient paymemtServiceClient, CustomerServiceClient customerServiceClient)
        {
            _context = context;
            _paymentServiceClient = paymemtServiceClient;
            _customerServiceClient = customerServiceClient;
        }

        [HttpGet]
        public IActionResult GetShipments()
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var shipments = _context.Shipments
                .Where(x => x.CustomerID == customerId)
                .ToList();

            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public IActionResult GetShipment(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var shipment = _context.Shipments.Find(id);

            if (shipment == null)
            {
                return NotFound("That Shipment does not exist");
            }

            if (shipment.CustomerID != customerId)
            {
                return Forbid();
            }

            return Ok(shipment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShipment(CreateShipmentRequest request)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var order = await _paymentServiceClient.GetOrder(request.OrderID);

            if (order == null)
            {
                return NotFound("That order does not exist");
            }

            if (order.CustomerID != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Paid")
            {
                return BadRequest("The order has not been paid");
            }

            var customer = await _customerServiceClient.GetCustomer(customerId);

            if (customer == null)
            {
                return NotFound("That customer does not exist");
            }

            var shipment = new Shipment
            {
                OrderID = request.OrderID,
                CustomerID = customerId,
                Name = customer.Name,
                Surname = customer.Surname,
                Phone = customer.Phone,
                Address = customer.Address,
                Status = "Order Received",
                CreatedAt = DateTime.Now
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            return Ok(shipment);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var shipment = _context.Shipments.Find(id);

            if (shipment == null)
            {
                return NotFound("No shipment with this id found");
            }

            var allowedStatuses = new[]
            {
        "Order Received",
        "Shipped",
        "In Transit",
        "Out for Delivery",
        "Delivered"
    };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest("Invalid shipment status");
            }

            shipment.Status = status;

            _context.SaveChanges();

            return Ok(shipment);
        }
    }
}
