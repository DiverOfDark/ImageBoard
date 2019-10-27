using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageBoard
{
  public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
                        
            IsProduction = Configuration["Properties:IsProduction"] == "true";
            CommmitHash = Configuration["Properties:CiCommitHash"];
            CommmitName = Configuration["Properties:CiCommitName"];
            BaseUri = Configuration["Properties:BaseUri"];
            TelegramToken = Configuration["Properties:Telegram"];
            CurrentTokenValidTill = DateTime.Now.AddYears(3);
        }

        public static string CommmitName { get; private set; }
        public static string CommmitHash { get; private set; }
        public static bool IsProduction { get; private set; }

        public IConfiguration Configuration { get; }
        public static string CurrentToken => "1234";
        
        public static String TelegramToken { get; private set; }
        public static String BaseUri { get; private set; }
        public static DateTime CurrentTokenValidTill { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddAuthorization();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x =>
            {
                x.AccessDeniedPath = "/Auth";
                x.LoginPath = "/Auth";
                x.LogoutPath = "/Logout";
            });
            services.AddProxy();

            services.AddSingleton<SavedSettings>();
            services.AddSingleton<TelegramBot>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets().RunProxy(new Uri(BaseUri));
            app.RunProxy(new Uri(BaseUri));
            app.ApplicationServices.GetService<TelegramBot>().Start();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{action=Index}/{id?}", defaults: new {controller = "Home" });
                endpoints.MapFallbackToController("Index", "Home");
            });

            app.Use((x, y) =>
            {
                x.Response.Redirect("/");
                return Task.CompletedTask;
            });
        }
    }
}