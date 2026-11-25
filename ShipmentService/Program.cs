using Microsoft.EntityFrameworkCore;
using ShipmentService.Data;
using ShipmentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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


builder.Services.AddDbContext<ShipmentServiceDbContext>(options => options.UseMySQL(builder.Configuration.GetConnectionString("ShipmentDb") ?? throw new InvalidOperationException("Connection string 'ShipmentDb' is not configured.")));
builder.Services.AddHttpClient<PaymemtServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService"] ?? throw new InvalidOperationException("Services:PaymentService is not configured."));
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 1;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 2;
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

builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
