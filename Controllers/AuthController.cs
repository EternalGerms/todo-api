using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace TodoApi.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IConfiguration _config;
        private static Dictionary<string, int> _refreshTokens = new(); // refreshToken -> userId
        public AuthController(TodoContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email already exists." });

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            _refreshTokens[refreshToken] = user.Id;
            return Ok(new AuthResponse { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.PasswordHash != HashPassword(request.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            _refreshTokens[refreshToken] = user.Id;
            return Ok(new AuthResponse { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("/refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!_refreshTokens.TryGetValue(request.RefreshToken, out int userId))
                return Unauthorized(new { message = "Invalid refresh token." });

            var user = _context.Users.Find(userId);
            if (user == null)
                return Unauthorized(new { message = "Invalid refresh token." });

            // Gera novo token e refresh token
            var token = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            _refreshTokens.Remove(request.RefreshToken);
            _refreshTokens[newRefreshToken] = user.Id;
            return Ok(new RefreshTokenResponse { Token = token, RefreshToken = newRefreshToken });
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "ThisIsASecretKeyForJwtToken";
            var jwtIssuer = _config["Jwt:Issuer"] ?? "TodoApiIssuer";
            var jwtAudience = _config["Jwt:Audience"] ?? "TodoApiAudience";
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 