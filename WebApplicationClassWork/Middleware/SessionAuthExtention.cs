using Microsoft.AspNetCore.Builder;

namespace WebApplicationClassWork.Middleware
{
    public static class SessionAuthExtention
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
