using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;

namespace TodoApi.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TodoContext _context;
        public AuthController(TodoContext context)
        {
            _context = context;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email already exists." });

            var token = Guid.NewGuid().ToString();

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                CurrentToken = token
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse { Token = token });
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.PasswordHash != HashPassword(request.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = Guid.NewGuid().ToString();
            user.CurrentToken = token;
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse { Token = token });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
} 