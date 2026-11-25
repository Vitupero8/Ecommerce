using CartService.Data;
using CartService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(document =>
    new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


// Add services to the container.
builder.Services.AddDbContext<CartDbContext>(options => options.UseMySQL(builder.Configuration.GetConnectionString("CartDb") ?? throw new InvalidOperationException("Connection string 'CartDb' is not configured.")));
builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? throw new InvalidOperationException("Services:ProductService is not configured."));
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 0;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 3;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? throw new InvalidOperationException("Services:ProductService is not configured."));
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 1;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 3;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"] ?? throw new InvalidOperationException("Services:CustomerService is not configured."));
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 1;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 3;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<PaymentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService"] ?? throw new InvalidOperationException("Services:PaymentService is not configured."));
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 1;
    options.Retry.DisableFor(HttpMethod.Post);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 3;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured."))
            )
        };
    });


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
