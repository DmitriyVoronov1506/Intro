using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using WebApplicationClassWork.Services;

namespace WebApplicationClassWork.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService, DAL.Context.IntroContext introContext)
        {
            string userId = context.Session.GetString("userId");    

            if (userId != null)
            {
                authService.Set(userId);
                
                long authMoment = Convert.ToInt64(context.Session.GetString("AuthMoment"));  
                long authInterval = (DateTime.Now.Ticks - authMoment) / (long)1e7;

                if (authInterval > 100)
                {
                    context.Session.Remove("userId");
                    context.Session.Remove("AuthMoment");

                    authService.User.LogMoment = DateTime.Now;
                    introContext.SaveChanges();

                    context.Response.Redirect("/");
                    return;
                }
            }
            //maincommit
            context.Items.Add("fromAuthMiddleware", "Hello !!");

            await next(context);
        }
    }
}
