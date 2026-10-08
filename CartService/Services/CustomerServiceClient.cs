using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using CartService.Models;

namespace CartService.Services
{
    public class CustomerServiceClient
    {

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public  CustomerServiceClient(HttpClient httpclient,  IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpclient;
              _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Customer> GetCustomer(int id)
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
