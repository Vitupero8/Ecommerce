using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProductService.Tests
{
    public class ProductIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProductIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetProducts_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Product");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}