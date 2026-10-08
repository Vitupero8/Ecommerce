using Microsoft.AspNetCore.Mvc;
using CartService.Data;
using CartService.Models;

namespace CartService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]


    public class CartItemController : ControllerBase
    {

        private readonly CartDbContext _context;

        public CartItemController(CartDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCartItem()
        {
            var CartItem = _context.CartItem.ToList();

            return Ok(CartItem);
        }

        [HttpGet("{id}")]
        public IActionResult GetWantedCartItem(int id)
        {
            var WantedCartItem = _context.CartItem.Find(id);

            if(WantedCartItem == null)
            {
                return NotFound();
            }

            return Ok(WantedCartItem);
        }

        [HttpPost]
        public IActionResult CreateCartItem(CartItems cartitem)
        {
            _context.CartItem.Add(cartitem);
            _context.SaveChanges();

            return Ok(cartitem);
            
        }

    }
}
