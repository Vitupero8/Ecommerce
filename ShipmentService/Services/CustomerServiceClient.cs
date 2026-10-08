using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using ShipmentService.Models;

namespace ShipmentService.Services
{
    public class CustomerServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerServiceClient(HttpClient httpClient , IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual async Task<Customer> GetCustomer(int id)
        {
            var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .ToString();

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Customer/{id}"
            );

            if (!string.IsNullOrEmpty(token))
            {
                httpRequest.Headers.Add("Authorization", token);
            }

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Customer>();
        }
    }
}
