using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;
using System.Text.Json; // For JsonSerializer

namespace TodoApi.Middleware
{
    public class TokenAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, TodoContext dbContext)
        {
            // Only apply to /todos path as per new requirements for todo item management
            if (context.Request.Path.StartsWithSegments("/todos"))
            {
                string token = context.Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Unauthorized" }));
                    return;
                }

                // Simple token validation: check if any user has this token
                // In a real app, you might want a more robust token (e.g., JWT with expiry)
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CurrentToken == token);

                if (user == null)
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Unauthorized" }));
                    return;
                }

                // Attach user to context
                context.Items["User"] = user;
            }

            await _next(context);
        }
    }
} 