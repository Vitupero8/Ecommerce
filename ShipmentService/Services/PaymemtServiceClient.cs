using System.Net.Http.Json;
using Mysqlx.Crud;
using ShipmentService.Models;
using Microsoft.AspNetCore.Http;
namespace ShipmentService.Services
{
    public class PaymemtServiceClient
    {

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymemtServiceClient(HttpClient httpClient,  IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual async Task<ShipmentService.Models.Order> GetOrder(int id)
        {
            var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .ToString();

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Order/{id}"
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

            return await response.Content.ReadFromJsonAsync<ShipmentService.Models.Order>();
        }


    }
}
