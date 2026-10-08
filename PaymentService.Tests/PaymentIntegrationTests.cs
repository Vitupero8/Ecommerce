using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PaymentService.Tests
{
    public class PaymentIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PaymentIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetOrders_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Order");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode
            );
        }
    }
}