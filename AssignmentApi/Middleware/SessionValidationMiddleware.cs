using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AssignmentApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentApi.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenSessionValueClaim = context.User.FindFirst("SessionValue")?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Middleware has a Singleton lifetime, but DbContext is Scoped.
                    // We resolve the DbContext from request services to avoid lifetime issues.
                    var dbContext = context.RequestServices.GetRequiredService<AssignmentDbContext>();
                    
                    var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                    
                    if (user == null || string.IsNullOrEmpty(user.SessionValue) || user.SessionValue != tokenSessionValueClaim)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new { status = false, message = "Active session invalid, expired, or logged out." });
                        return; // Short-circuit the request pipeline
                    }
                }
            }

            await _next(context);
        }
    }
}
