using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageBoard
{
  public class Startup
    {
        private Timer _timer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
                        
            IsProduction = Configuration["Properties:IsProduction"] == "true";
            CommmitHash = Configuration["Properties:CiCommitHash"];
            CommmitName = Configuration["Properties:CiCommitName"];
            BaseUri = Configuration["Properties:BaseUri"];
            TelegramToken = Configuration["Properties:Telegram"];
            _timer = new Timer(UpdateToken, null, TimeSpan.Zero, TimeSpan.FromHours(4));
        }

        private void UpdateToken(object state)
        {
            var pin = new Random(Environment.TickCount).Next() % 1000000;
            CurrentToken = pin.ToString("000000");
            CurrentTokenValidTill = DateTime.Now.AddHours(4);
        }

        public static string CommmitName { get; private set; }
        public static string CommmitHash { get; private set; }
        public static bool IsProduction { get; private set; }

        public IConfiguration Configuration { get; }
        public static string CurrentToken {get; private set; }
        
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