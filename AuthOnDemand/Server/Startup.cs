using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthOnDemand.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment WebHostEnvironment;

        public void ConfigureServices(IServiceCollection services)
        {
            // Read this from secure configuration
            var clientId = "xx";
            var clientSecret = "xx";

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)

                  // See LoginController for how to trigger and handle this authentication method
                  .AddGoogle("Google", options =>
                  {
                      options.ClientId = clientId;
                      options.ClientSecret = clientSecret;

                      // Need access to gmail labels? See scopes in https://developers.google.com/identity/protocols/oauth2/scopes
                      options.Scope.Add("https://www.googleapis.com/auth/gmail.labels");

                      // Do you need a refresh token? Use this
                      // Warning: Google gives you only one refresh token, 
                      // so you set it but you wan't always get a refresh token
                      // See https://developers.google.com/identity/protocols/oauth2 -> Refresh token expiration
                      options.AccessType = "offline";

                      // Use this if you need an access o refresh token
                      // If you need only the identity remove this line, it will make your cookie smaller
                      options.SaveTokens = true;

                      if (WebHostEnvironment.IsDevelopment())
                      {
                          // For debugging purposes only, so you can check how auth cookie flows
                          options.CorrelationCookie.Name = "googlecookie";
                      }
                  });

            services.AddAuthorization();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
