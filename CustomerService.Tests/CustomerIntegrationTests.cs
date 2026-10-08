using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerService.Tests
{
    public class CustomerIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CustomerIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetCustomers_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Customer");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}