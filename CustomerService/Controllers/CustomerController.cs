using CustomerService.Data;
using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IPasswordHasher<Customer> _passwordHasher;

        private readonly JwtService _jwtService;

        public CustomerController(CustomerDbContext context, IPasswordHasher<Customer> passwordHasher, JwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult GetCustomers()
        {
            var customers = _context.Customers.ToList();

            return Ok(customers);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetCustomer(int id)
        {

            var customerIdClaim = User.FindFirst("CustomerID");

            if (customerIdClaim == null)
            {
                return Unauthorized();
            }

            int customerId = int.Parse(customerIdClaim.Value);

            if (customerId != id)
            {
                return Forbid();
            }

            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return NotFound();
            }

            var response = new CustomerResponse
            {
                CustomerID = customer.CustomerID,
                Name = customer.Name,
                Surname = customer.Surname,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address
            };

            return Ok(response);
        }
        [HttpPost]
        public IActionResult CreateCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();

            return Ok(customer);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id , Customer customer)
        {
            var existingCustomer = _context.Customers.Find(id);

            if(existingCustomer == null)
            {
                return NotFound();
            }

            existingCustomer.Name = customer.Name;
            existingCustomer.Surname = customer.Surname;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;

            _context.SaveChanges();

            return Ok(existingCustomer);
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            var existingCustomer = _context.Customers.FirstOrDefault(x => x.Email == request.Email);

            if(existingCustomer != null)
            {
                return BadRequest("Email is already registered");
            }

            var customer = new Customer
            {
                Name = request.Name,
                Surname = request.Surname,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email
            };

            customer.PasswordHash = _passwordHasher.HashPassword(
                customer,
                request.Password
                );

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return Ok(customer);

        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var customer = _context.Customers
                .FirstOrDefault(x => x.Email == request.Email);

            if (customer == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                customer,
                customer.PasswordHash,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password");
            }

            var token = _jwtService.CreateToken(customer);

            return Ok(token);
        }


    }
}