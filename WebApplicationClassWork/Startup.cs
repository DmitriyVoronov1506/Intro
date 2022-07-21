using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationClassWork.Middleware;
using WebApplicationClassWork.Services;

namespace WebApplicationClassWork
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DAL.Context.IntroContext>(options => options.UseSqlServer(Configuration.GetConnectionString("IntroDb")));
            services.AddControllersWithViews();
            services.AddSingleton<RandomService>();
            services.AddSingleton<IHasher, ShaHasher>();
            
            services.AddSingleton<ICurrentDate, CurrentDateUtc>(); // Если хотите изменить тип отображения данных, замените CurrentDateUtc на CurrentDate

            // Singlton потому что у нас геттер, который при каждом обращении получает текущюю дату и время. Нет нужды пересоздавать объект каждый раз.

            services.AddScoped<IAuthService, SessionAuthService>();

            services.AddDistributedMemoryCache();  //Session

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(24);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
          
            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            //app.UseMiddleware<Middleware.SessionAuthMiddleware>();
            app.UseSessionAuth();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
