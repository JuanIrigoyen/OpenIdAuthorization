using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace MvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication(setup =>
            {
                setup.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                setup.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(setup =>
            {
                setup.Cookie.Name = "AppCookie";
                setup.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                //setup.Cookie.Expiration = TimeSpan.FromSeconds(10);
            })
            .AddOpenIdConnect(setup =>
            {
                // Setup own identity server
                setup.Authority = "https://localhost:44303";
                setup.ClientId = "hybrid-sample";
                setup.ClientSecret = "secret";
                setup.ResponseType = "code id_token";
               
                setup.Scope.Clear();
                setup.Scope.Add("openid");
                setup.Scope.Add("profile");
                setup.Scope.Add("email");
                setup.Scope.Add("sampleapi");
                setup.Scope.Add("workshopprofile");

                setup.SaveTokens = true;

                //// let you to get the user information in diferent channels
                //// setup.GetClaimsFromUserInfoEndpoint = true;

                //// setup.ClaimActions.Remove("WorkshopClaimType");
                //// setup.ClaimActions.MapUniqueJsonKey("pepito", "WorkshopClaimType");

                //setup.CallbackPath = "/callback";
                setup.SignedOutCallbackPath = "/signout-callback-oidc";
                setup.SignedOutRedirectUri = "/home/index";

                setup.ClaimActions.MapUniqueJsonKey("pepito", "WorkshopClaimType");

                //// Setup names claims and roles for found when Microsoft is not the provider
                //setup.TokenValidationParameters = new TokenValidationParameters()
                //{
                //    ClockSkew = new TimeSpan(10000),
                //    NameClaimType = "claim",
                //    RoleClaimType = "role",
                //    RequireSignedTokens = false
                //};

                // To force the client sign if you close the browser
                //setup.Events.OnRedirectToIdentityProvider = context =>
                //{
                //    context.ProtocolMessage.Prompt = Configuration["Security:Prompt"];
                //    return Task.CompletedTask;
                //};

                //// Setup names claims and roles for found when Microsoft is not the provider
                //setup.TokenValidationParameters = new TokenValidationParameters()
                //{
                //    ClockSkew = new TimeSpan(10000)
                //};

                // Use when you need that timelife cookie is the same that the Token. It´s a good practice
                setup.UseTokenLifetime = true;

                //// Let you to open page with last state
                //setup.Events.OnTicketReceived = context =>
                //{
                //    context.Properties.IsPersistent = true;
                //    return Task.CompletedTask;
                //};
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
