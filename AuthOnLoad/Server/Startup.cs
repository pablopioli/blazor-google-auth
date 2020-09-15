using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GoogleAuth.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Read this from secure configuration
            var clientId = "xx";
            var clientSecret = "xx";

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Google";
            })
                  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                  .AddGoogle("Google", options =>
                        {
                            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                            options.ClientId = clientId;
                            options.ClientSecret = clientSecret;

                            // Need access to gmail labels? See scopes in https://developers.google.com/identity/protocols/oauth2/scopes
                            options.Scope.Add("https://www.googleapis.com/auth/gmail.labels");

                            // Use this if you need an ACCESS token
                            // If you need only the identity remove this line, it will make your cookie smaller
                            options.SaveTokens = true;
                        });

            services.AddAccessTokenManagement();

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = 
                    new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);

                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder
                    .RequireAuthenticatedUser();

                options.FallbackPolicy = defaultAuthorizationPolicyBuilder.Build();
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
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
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages().RequireAuthorization();
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapFallbackToFile("index.html").RequireAuthorization();
            });
        }
    }
}
