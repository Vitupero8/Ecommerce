using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using ProductService.Data;

using ProductService.Models;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {

        private readonly ProductDbContext _context;

        public ProductController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            var Products = _context.Products.ToList();

            return Ok(Products);
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var Product = _context.Products.Find(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Ok(Product);
        }

        [HttpPost]
       
        public IActionResult CreateProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var Product = _context.Products.Find(id);

            if(Product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(Product);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Product product , int id)
        {
            var existingProduct = _context.Products.Find(id);

            if(existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductPrice = product.ProductPrice;
            existingProduct.ProductStockQuantity = product.ProductStockQuantity;
            existingProduct.ProductType = product.ProductType;

            _context.SaveChanges();

            return Ok(existingProduct);
        }


    }
}
