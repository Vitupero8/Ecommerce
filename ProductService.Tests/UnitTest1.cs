using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Controllers;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Tests
{
    public class UnitTest1
    {
        private ProductDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ProductDbContext(options);
        }

        [Fact]
        public void GetProductById_ReturnsProduct()
        {
            var context = CreateDatabase();

            var product = new Product
            {
                ProductID = 1,
                ProductName = "Logitech G502",
                ProductType = "Gaming Mouse",
                ProductPrice = 59.99m,
                ProductStockQuantity = 25
            };

            context.Products.Add(product);
            context.SaveChanges();

            var controller = new ProductController(context);

            var result = controller.GetProductById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(1, returnedProduct.ProductID);
            Assert.Equal("Logitech G502", returnedProduct.ProductName);
            Assert.Equal(59.99m, returnedProduct.ProductPrice);
        }

        [Fact]
        public void GetProductById_ReturnsNotFound()
        {
            var context = CreateDatabase();

            var controller = new ProductController(context);

            var result = controller.GetProductById(999);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);

            Assert.NotNull(notFoundResult);
        }
    }
}