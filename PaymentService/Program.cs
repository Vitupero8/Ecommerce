using Microsoft.EntityFrameworkCore;
using PaymentService.Services;
using PaymentService.Data;
using PaymentService.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<PaymentServiceDbContext>(options => options.UseMySQL("Server=localhost;Port=3306;Database=PaymentService;User=root;Password=Route123!;"));
builder.Services.AddHttpClient<CartServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7211/");
});
builder.Services.AddHttpClient<PayPalService>(client =>
{
    client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com/");
});
builder.Services.AddScoped<CardPaymentService>();
builder.Services.AddScoped<ApplePayService>();
builder.Services.AddScoped<BankTransferService>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
