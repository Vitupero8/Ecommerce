using CustomerService.Controllers;
using CustomerService.Data;
using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Tests
{
    public class UnitTest1
    {
        private CustomerDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CustomerDbContext(options);
        }

        private IConfiguration CreateConfiguration()
        {
            var settings = new Dictionary<string, string>
            {
                { "Jwt:Key", "THIS-IS-A-DEVELOPMENT-SECRET-CHANGE-IT" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void CreateToken_ReturnsToken()
        {
            var configuration = CreateConfiguration();

            var jwtService = new JwtService(configuration);

            var customer = new Customer
            {
                CustomerID = 5,
                Email = "testcustomer5@example.com"
            };

            var token = jwtService.CreateToken(customer);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void Register_CreatesCustomer()
        {
            var context = CreateDatabase();

            var passwordHasher = new PasswordHasher<Customer>();

            var jwtService = new JwtService(CreateConfiguration());

            var controller = new CustomerController(
                context,
                passwordHasher,
                jwtService
            );

            var request = new RegisterRequest
            {
                Name = "John",
                Surname = "Test",
                Phone = "067111111",
                Address = "Test Street",
                Email = "john@test.com",
                Password = "Test123!"
            };

            var result = controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var customer = Assert.IsType<Customer>(okResult.Value);

            Assert.Equal("John", customer.Name);
            Assert.Equal("john@test.com", customer.Email);
            Assert.NotEmpty(customer.PasswordHash);
        }

        [Fact]
        public void Register_RejectsExistingEmail()
        {
            var context = CreateDatabase();

            var existingCustomer = new Customer
            {
                Name = "Existing",
                Surname = "Customer",
                Phone = "067222222",
                Address = "Existing Street",
                Email = "existing@test.com",
                PasswordHash = "somehash"
            };

            context.Customers.Add(existingCustomer);
            context.SaveChanges();

            var passwordHasher = new PasswordHasher<Customer>();
            var jwtService = new JwtService(CreateConfiguration());

            var controller = new CustomerController(
                context,
                passwordHasher,
                jwtService
            );

            var request = new RegisterRequest
            {
                Name = "Another",
                Surname = "Customer",
                Phone = "067333333",
                Address = "Another Street",
                Email = "existing@test.com",
                Password = "Test123!"
            };

            var result = controller.Register(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Email is already registered",
                badRequest.Value
            );
        }

        [Fact]
        public void Login_WithCorrectPassword_ReturnsToken()
        {
            var context = CreateDatabase();

            var passwordHasher = new PasswordHasher<Customer>();

            var customer = new Customer
            {
                CustomerID = 5,
                Name = "Test",
                Surname = "Customer",
                Phone = "067123456",
                Address = "Test Street",
                Email = "test@test.com"
            };

            customer.PasswordHash = passwordHasher.HashPassword(
                customer,
                "Test123!"
            );

            context.Customers.Add(customer);
            context.SaveChanges();

            var jwtService = new JwtService(CreateConfiguration());

            var controller = new CustomerController(
                context,
                passwordHasher,
                jwtService
            );

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Test123!"
            };

            var result = controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var token = Assert.IsType<string>(okResult.Value);

            Assert.NotEmpty(token);
        }

        [Fact]
        public void Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var context = CreateDatabase();

            var passwordHasher = new PasswordHasher<Customer>();

            var customer = new Customer
            {
                CustomerID = 5,
                Name = "Test",
                Surname = "Customer",
                Phone = "067123456",
                Address = "Test Street",
                Email = "test@test.com"
            };

            customer.PasswordHash = passwordHasher.HashPassword(
                customer,
                "CorrectPassword123!"
            );

            context.Customers.Add(customer);
            context.SaveChanges();

            var jwtService = new JwtService(CreateConfiguration());

            var controller = new CustomerController(
                context,
                passwordHasher,
                jwtService
            );

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "WrongPassword123!"
            };

            var result = controller.Login(request);

            var unauthorizedResult =
                Assert.IsType<UnauthorizedObjectResult>(result);

            Assert.Equal(
                "Invalid email or password",
                unauthorizedResult.Value
            );
        }
    }
}