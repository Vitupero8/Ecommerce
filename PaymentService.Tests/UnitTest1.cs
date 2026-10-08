using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Controllers;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Tests
{
    public class UnitTest1
    {
        private PaymentServiceDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<PaymentServiceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new PaymentServiceDbContext(options);
        }

        private OrderController CreateController(PaymentServiceDbContext context)
        {
            var controller = new OrderController(
                context,
                null,
                null,
                null,
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
        public void GetOrder_ReturnsOrder()
        {
            var context = CreateDatabase();

            var order = new Order
            {
                OrderId = 1,
                CartID = 13,
                CustomerID = 5,
                TotalPrice = 59.99m,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            context.Order.Add(order);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.GetOrder(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOrder = Assert.IsType<Order>(okResult.Value);

            Assert.Equal(1, returnedOrder.OrderId);
            Assert.Equal(5, returnedOrder.CustomerID);
            Assert.Equal(59.99m, returnedOrder.TotalPrice);
            Assert.Equal("Pending", returnedOrder.Status);
        }

        [Fact]
        public void GetOrder_ReturnsNotFound()
        {
            var context = CreateDatabase();

            var controller = CreateController(context);

            var result = controller.GetOrder(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}