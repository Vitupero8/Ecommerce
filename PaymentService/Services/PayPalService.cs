using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PaymentService.Services
{
    public class PayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayPalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessToken()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")
            );

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            var content = new StringContent(
                "grant_type=client_credentials",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            var response = await _httpClient.PostAsync(
                "v1/oauth2/token",
                content
            );

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }

            using var json = JsonDocument.Parse(result);

            return json.RootElement
                .GetProperty("access_token")
                .GetString();
        }

        public async Task<string> CreatePayPalOrder(decimal amount)
        {
            var token = await GetAccessToken();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var order = new
            {
                intent = "CAPTURE",

                purchase_units = new[]
                {
            new
            {
                amount = new
                {
                    currency_code = "USD",
                    value = amount.ToString("0.00")
                  
                }
            }
        },

                application_context = new
                {
                    return_url = "https://localhost:7112/api/Order/paypal-success",
                    cancel_url = "https://localhost:7112/api/Order/paypal-cancel"
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v2/checkout/orders",
                order
            );

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }

            return result;
        }

        public async Task<string> CapturePayPalOrder(string orderId)
        {
            var token = await GetAccessToken();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                "{}",
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"v2/checkout/orders/{orderId}/capture",
                content
            );

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }

            return result;
        }
    }
}
