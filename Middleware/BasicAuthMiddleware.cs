using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Middleware
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, TodoContext dbContext)
        {
            if (context.Request.Path.StartsWithSegments("/api/tasks"))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                    var decodedBytes = Convert.FromBase64String(encodedUsernamePassword);
                    var decodedUsernamePassword = Encoding.UTF8.GetString(decodedBytes);
                    var parts = decodedUsernamePassword.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var username = parts[0];
                        var password = parts[1];
                        var passwordHash = HashPassword(password);
                        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash);
                        if (user != null)
                        {
                            context.Items["User"] = user;
                            await _next(context);
                            return;
                        }
                    }
                }
                context.Response.StatusCode = 401;
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"TodoApi\"";
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            await _next(context);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
} 