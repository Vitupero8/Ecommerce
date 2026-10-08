using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using PaymentService.Services;

namespace PaymentService.Tests
{
    public class PayPalServiceTests
    {
        [Fact]
        public async Task GetAccessToken_ReturnsAccessToken()
        {
            var handler = new MockHttpMessageHandler();

            handler.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"access_token\":\"test-token-123\"}",
                    Encoding.UTF8,
                    "application/json"
                )
            };
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api-m.sandbox.paypal.com/")
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PayPal:ClientId"] = "test-client",
                    ["PayPal:ClientSecret"] = "test-secret"
                })
                .Build();

            var service = new PayPalService(
                httpClient,
                configuration
            );

            var token = await service.GetAccessToken();

            Assert.Equal("test-token-123", token);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Response);
        }
    }
}