using System.Net.Http.Json;
using CartService.Models;
using Microsoft.AspNetCore.Http;
namespace CartService.Services
{
    public class PaymentServiceClient
    {
        private readonly HttpClient _httpclient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentServiceClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _httpclient = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CreateOrder(CheckoutRequest request)
        {
            var token = _httpContextAccessor.HttpContext?
        .Request.Headers["Authorization"]
        .ToString();

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Order"
            );

            httpRequest.Content = JsonContent.Create(request);

            if (!string.IsNullOrEmpty(token))
            {
                httpRequest.Headers.Add("Authorization", token);
            }

            var response = await _httpclient.SendAsync(httpRequest);

            var result = await response.Content.ReadAsStringAsync();

            return $"{(int)response.StatusCode}: {result}";
        }
    }
}
