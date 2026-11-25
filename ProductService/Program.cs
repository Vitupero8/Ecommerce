using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ProductDbContext>(options => options.UseMySQL(builder.Configuration.GetConnectionString("ProductDb") ?? throw new InvalidOperationException("Connection string 'ProductDb' is not configured."))); 

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
