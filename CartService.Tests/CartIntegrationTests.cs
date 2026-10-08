using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CartService.Tests
{
    public class CartIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CartIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetCart_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Cart/1");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode
            );
        }
    }
}