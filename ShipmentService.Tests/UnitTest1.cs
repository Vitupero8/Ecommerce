using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentService.Controllers;
using ShipmentService.Data;
using ShipmentService.Models;
using ShipmentService.Services;

namespace ShipmentService.Tests
{
    public class UnitTest1
    {
        private ShipmentServiceDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<ShipmentServiceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ShipmentServiceDbContext(options);
        }

        private ShipmentController CreateController(
            ShipmentServiceDbContext context)
        {
            var controller = new ShipmentController(
                context,
                null,
                null
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

            return controller;
        }

        [Fact]
        public void GetShipment_ReturnsShipment()
        {
            var context = CreateDatabase();

            var shipment = new Shipment
            {
                ShipmentID = 1,
                OrderID = 21,
                CustomerID = 5,
                Name = "Petar",
                Surname = "Milic",
                Phone = "069123456",
                Address = "Podgorica",
                Status = "Order Received",
                CreatedAt = DateTime.Now
            };

            context.Shipments.Add(shipment);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.GetShipment(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedShipment = Assert.IsType<Shipment>(okResult.Value);

            Assert.Equal(1, returnedShipment.ShipmentID);
            Assert.Equal(21, returnedShipment.OrderID);
            Assert.Equal(5, returnedShipment.CustomerID);
            Assert.Equal("Order Received", returnedShipment.Status);
        }

        [Fact]
        public void GetShipment_ReturnsNotFound()
        {
            var context = CreateDatabase();

            var controller = CreateController(context);

            var result = controller.GetShipment(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetShipment_ReturnsForbidForAnotherCustomer()
        {
            var context = CreateDatabase();

            var shipment = new Shipment
            {
                ShipmentID = 1,
                OrderID = 21,
                CustomerID = 10,
                Name = "Other",
                Surname = "Customer",
                Phone = "069000000",
                Address = "Podgorica",
                Status = "Order Received",
                CreatedAt = DateTime.Now
            };

            context.Shipments.Add(shipment);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.GetShipment(1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}