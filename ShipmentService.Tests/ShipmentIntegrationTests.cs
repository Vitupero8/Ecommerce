using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ShipmentService.Tests
{
    public class ShipmentIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ShipmentIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetShipments_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Shipment");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode
            );
        }
    }
}