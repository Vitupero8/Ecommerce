
using CartService.Data;
using CartService.Models;
using CartService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace CartService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class CartController : ControllerBase
    {

        private readonly CartDbContext _context;
        private readonly ProductServiceClient _productServiceClient;

        private readonly CustomerServiceClient _customerServiceClient;

        private readonly PaymentServiceClient _paymentServiceClient;

        public CartController(CartDbContext context, ProductServiceClient productServiceClient, CustomerServiceClient customerServiceClient, PaymentServiceClient paymentServiceClient)
        {
            _context = context;
            _productServiceClient = productServiceClient;
            _customerServiceClient = customerServiceClient;
            _paymentServiceClient = paymentServiceClient;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetCarts()
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var Cart = _context.Cart
                .Where(x => x.CustomerID == customerId)
                .ToList();

            return Ok(Cart);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetCart(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var WantedCart = _context.Cart.Find(id);

            if (WantedCart == null)
            {
                return NotFound();
            }

            if (WantedCart.CustomerID != customerId)
            {
                return Forbid();
            }

            return Ok(WantedCart);
        }

        /*[HttpPost]
        public IActionResult CreateCart(Carts cart)
        {
            _context.Cart.Add(cart);
            _context.SaveChanges();

            return Ok(cart);
        }*/

        [Authorize]
        [HttpPost]
        public IActionResult CreateCart(Carts cart)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            cart.CustomerID = customerId;

            _context.Cart.Add(cart);
            _context.SaveChanges();

            return Ok(cart);
        }


        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProductFromProductService(int id)
        {
            var product = await _productServiceClient.GetProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [Authorize]
        [HttpPost("product")]
        public async Task<IActionResult> AddProductToCart(AddToCartRequest request)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var product = await _productServiceClient.GetProduct(request.ProductId);

            if (product == null)
            {
                return NotFound("Product does not exist");
            }

            if (product.ProductStockQuantity <= 0)
            {
                return BadRequest("The Product is no longer available");
            }

            if (product.ProductStockQuantity < request.Quantity)
            {
                return BadRequest("Not enough Stock ");
            }

            var cart = _context.Cart.Find(request.CartId);

            if (cart == null)
            {
                return NotFound("Cart does not exist");
            }

            if (cart.CustomerID != customerId)
            {
                return Forbid();
            }

            var cartItem = new CartItems
            {
                CartId = request.CartId,
                ProductID = request.ProductId,
                Quantity = request.Quantity,
                Price = product.ProductPrice
            };

            _context.CartItem.Add(cartItem);

            cart.TotalPrice += product.ProductPrice * request.Quantity;

            _context.SaveChanges();

            return Ok(cartItem);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> ChangeQuantityCartItem(int id, ChangeQuantityRequest request)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var CartItem = _context.CartItem.Find(id);

            if (CartItem == null)
            {
                return NotFound("CartItem does not exist");
            }

            if (request == null || request.Quantity <= 0)
            {
                return BadRequest("The quantity cannot be empty or less than 0!");
            }

            var Cart = _context.Cart.Find(CartItem.CartId);

            if (Cart == null)
            {
                return NotFound("Cart does not exist!");
            }

            if (Cart.CustomerID != customerId)
            {
                return Forbid();
            }

            var product = await _productServiceClient.GetProduct(CartItem.ProductID);

            if (product == null)
            {
                return NotFound("Product has not been found!");
            }

            if (product.ProductStockQuantity < request.Quantity)
            {
                return BadRequest("The product does not have that many listings in stock!");
            }

            if (CartItem.Quantity == request.Quantity)
            {
                return BadRequest("You cannot change the quantity to the same one!");
            }

            var oldQuantity = CartItem.Quantity;
            var oldPrice = CartItem.Price;

            CartItem.Quantity = request.Quantity;
            CartItem.Price = product.ProductPrice;

            Cart.TotalPrice =
                Cart.TotalPrice
                - (oldQuantity * oldPrice)
                + (request.Quantity * product.ProductPrice);

            _context.SaveChanges();

            return Ok(Cart);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteCartItem(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var CartItem = _context.CartItem.Find(id);

            if (CartItem == null)
            {
                return NotFound("CartItem does not exist!");
            }

            var Cart = _context.Cart.Find(CartItem.CartId);

            if (Cart == null)
            {
                return NotFound("Cart not found!");
            }

            if (Cart.CustomerID != customerId)
            {
                return Forbid();
            }

            Cart.TotalPrice = Cart.TotalPrice - (CartItem.Price * CartItem.Quantity);

            _context.Remove(CartItem);
            _context.SaveChanges();

            return Ok(Cart);
        }


        [Authorize]
        [HttpPost("{id}")]
        public async Task<IActionResult> Checkout(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            var Cart = _context.Cart.Find(id);

            if (Cart == null)
            {
                return NotFound("Cart does not exist");
            }

            if (Cart.CustomerID != customerId)
            {
                return Forbid();
            }

            Cart.TotalPrice = 0;

            var CartItems = await _context.CartItem
                .Where(x => x.CartId == Cart.CartId)
                .ToListAsync();

            if (CartItems.Count == 0)
            {
                return BadRequest("You dont have anything in your Cart!");
            }

            foreach (var CartItem in CartItems)
            {
                var product = await _productServiceClient.GetProduct(CartItem.ProductID);

                if (product == null)
                {
                    return NotFound("A product in your Cart is no longer available!");
                }

                if (product.ProductStockQuantity < CartItem.Quantity)
                {
                    return BadRequest("The stock is less than the provided quantity!");
                }

                Cart.TotalPrice += product.ProductPrice * CartItem.Quantity;

                if (product.ProductPrice != CartItem.Price)
                {
                    CartItem.Price = product.ProductPrice;
                }
            }

            _context.SaveChanges();

            var request = new CheckoutRequest
            {
                CartId = Cart.CartId,
                CustomerId = Cart.CustomerID,
                TotalPrice = Cart.TotalPrice,
                Items = CartItems.Select(x => new CheckoutItem
                {
                    ProductId = x.ProductID,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            };

            var orderResult = await _paymentServiceClient.CreateOrder(request);

            return Ok(orderResult);
        }



    }
}
