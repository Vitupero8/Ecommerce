using System.Net.Http.Json;
using CartService.Models;

namespace CartService.Services
{
    public class ProductServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual async Task<Product> GetProduct(int id)
        {
            var response = await _httpClient.GetAsync($"api/Product/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Product>();
        }
    }
}