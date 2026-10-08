using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ShipmentService.Controllers;
using ShipmentService.Data;
using ShipmentService.Models;
using ShipmentService.Services;

namespace ShipmentService.Tests
{
    public class ShipmentMockTests
    {
        private ShipmentServiceDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<ShipmentServiceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ShipmentServiceDbContext(options);
        }

        [Fact]
        public async Task CreateShipment_CreatesShipment()
        {
            var context = CreateDatabase();

            var paymentServiceMock = new Mock<PaymemtServiceClient>(
                new HttpClient(),
                new HttpContextAccessor()
            );

            paymentServiceMock
                .Setup(x => x.GetOrder(21))
                .ReturnsAsync(new ShipmentService.Models.Order
                {
                    OrderId = 21,
                    CustomerID = 5,
                    Status = "Paid"
                });

            var customerServiceMock = new Mock<CustomerServiceClient>(
                new HttpClient(),
                new HttpContextAccessor()
            );

            customerServiceMock
                .Setup(x => x.GetCustomer(5))
                .ReturnsAsync(new Customer
                {
                    CustomerID = 5,
                    Name = "Petar",
                    Surname = "Milic",
                    Phone = "069123456",
                    Address = "Podgorica"
                });

            var controller = new ShipmentController(
                context,
                paymentServiceMock.Object,
                customerServiceMock.Object
            );

            var httpContext = new DefaultHttpContext();

            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[]
                    {
                        new System.Security.Claims.Claim("CustomerID", "5")
                    },
                    "Test"
                )
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var request = new CreateShipmentRequest
            {
                OrderID = 21
            };

            var result = await controller.CreateShipment(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var shipment = Assert.IsType<Shipment>(okResult.Value);

            Assert.Equal(21, shipment.OrderID);
            Assert.Equal(5, shipment.CustomerID);
            Assert.Equal("Petar", shipment.Name);
            Assert.Equal("Milic", shipment.Surname);
            Assert.Equal("069123456", shipment.Phone);
            Assert.Equal("Podgorica", shipment.Address);
            Assert.Equal("Order Received", shipment.Status);

            paymentServiceMock.Verify(
                x => x.GetOrder(21),
                Times.Once
            );

            customerServiceMock.Verify(
                x => x.GetCustomer(5),
                Times.Once
            );
        }
    }
}