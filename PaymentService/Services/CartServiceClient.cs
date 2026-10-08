using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PaymentService.Models;
namespace PaymentService.Services
{
    public class CartServiceClient
    {
        private readonly HttpClient _httpclient;

        public CartServiceClient(HttpClient httpclient)
        {
            _httpclient = httpclient;
        }

        public async Task<Cart> GetCart(int id)
        {
            var response = await _httpclient.GetAsync($"api/Cart/{id}");

            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Cart>();
        }
        
        public async Task<CartItems> GetCartItem(int id)
        {
            var response = await _httpclient.GetAsync($"api/CartItems/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CartItems>();
        }
    }
}
