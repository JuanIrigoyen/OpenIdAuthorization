using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenIdAutorizationServer
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
            services.AddControllersWithViews()
                .Services
                .AddAuthentication().AddOpenIdConnect(setup =>
                {
                    setup.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    setup.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    setup.ClientId = "0c6af2dc - 41ca - 40f3 - 9bdb - 99839e0d3f55";
                    setup.ClientSecret = ":5Rgl1WRLB9f /[eFbNDO2M / KuZD0kiDF";
                    setup.Authority = "https://login.windows.com/demosbeezy.onmicrosoft.com";
                    setup.RequireHttpsMetadata = true;
                })
                .Services
                .AddIdentityServer(setup =>
                {
                    //setup.Discovery.ShowEndpoints = true;
                    //setup.Discovery.ShowGrantTypes = false;
                    //setup.DeviceFlow.Interval = 10;

                    //setup.UserInteraction.LoginUrl = @"\acctimespanount\login";
                    //setup.Authentication.CookieLifetime = TimeSpan.FromMinutes(5);

                    //// Every time that use the web, the cookie time in cookieLifeTime begin again
                    //setup.Authentication.CookieSlidingExpiration = true;


                    //// It´s not http only, only access from javascript
                    //setup.Authentication.CheckSessionCookieName = "test";

                    //// Setup content security police
                    //setup.Csp.Level = CspLevel.One;

                    //// Setup Cors
                    //setup.Endpoints.EnableTokenRevocationEndpoint = true;

                    //// Use in containers you need setup for discovery data and other operations
                    //setup.PublicOrigin = "http://aaaa";
                    //setup.LowerCaseIssuerUri = true;

                    //// Events contains information suscriptions 
                    //setup.Events.RaiseErrorEvents = true;

                })
                .AddInMemoryIdentityResources(new List<IdentityResource>()
                {
                    new IdentityResource()
                    {
                        DisplayName = "Workshop User Profile",
                        Name = "workshopprofile",
                        Description = "The identity profile for workshop",
                        ShowInDiscoveryDocument = true,
                        UserClaims = { "WorkshopClaimType" }
                    },
                    new IdentityResources.Profile(),
                    new IdentityResources.OpenId(),
                    new IdentityResources.Email(),
                })
                .AddTestUsers(new List<TestUser>()
                {
                    new TestUser()
                    {
                        SubjectId = "juan.irigoyen@oranauto.com",
                        Username = "workshop",
                        Password = "workshop",
                        IsActive = true,
                        Claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Email, "juan.irigoyen@oranauto.com"),
                            new Claim(JwtClaimTypes.Email, "juanwt@oranauto.com"),
                            new Claim(JwtClaimTypes.FamilyName, "juan"),
                            new Claim(JwtClaimTypes.GivenName, "juan"),
                            new Claim("WorkshopClaimType", "Workshop claim type value"),
                        }
                    }
                })
                .AddInMemoryClients(new List<Client>()
                {
                    new Client()
                    {
                        ClientName = "hybrid-sample",
                        ClientId = "hybrid-sample",
                        ClientSecrets = { new Secret("secret".Sha256())},
                        AccessTokenType = AccessTokenType.Jwt,
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            //IdentityServerConstants.StandardScopes.OfflineAccess,
                            "sampleapi",
                            "workshopprofile"
                        },
                        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                        AllowOfflineAccess = false,
                        RedirectUris = { "https://localhost:44317/signin-oidc", "https://localhost:44317/callback"},
                        RequireClientSecret = false,
                       
                        // if not true remove claims not included in the token
                        AlwaysIncludeUserClaimsInIdToken = false,
                        AllowAccessTokensViaBrowser = false,

                        AccessTokenLifetime = 50,
                        IdentityTokenLifetime = 50,
                        UserSsoLifetime = 50,

                         // Use only if not need Require Consent Form from users
                        RequireConsent = false,
                        PostLogoutRedirectUris = {"https://localhost:44317/signout-callback-oidc"},

                        // Use if identity server logout to close other 
                        FrontChannelLogoutUri = "https://localhost:44317/home/frontchannellogout",
                        BackChannelLogoutUri  = "https://localhost:44317/home/backchannellogout",
                    },
                    new Client()
                    {
                        ClientName = "hybrid-sample2",
                        ClientId = "hybrid-sample2",
                        ClientSecrets = { new Secret("secret".Sha256())},
                        AccessTokenType = AccessTokenType.Jwt,
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            //IdentityServerConstants.StandardScopes.OfflineAccess,
                            "sampleapi",
                            "workshopprofile"
                        },
                        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                        AllowOfflineAccess = false,
                        RedirectUris = { "https://localhost:44318/signin-oidc", "https://localhost:44318/callback"},
                        RequireClientSecret = false,
                       
                        // if not true remove claims not included in the token
                        AlwaysIncludeUserClaimsInIdToken = false,
                        AllowAccessTokensViaBrowser = false,

                        AccessTokenLifetime = 50,
                        IdentityTokenLifetime = 50,
                        UserSsoLifetime = 50,

                        // Use only if not need Require Consent Form from users
                        RequireConsent = false,
                        PostLogoutRedirectUris = {"https://localhost:44318/signout-callback-oidc"},

                        // Use if identity server logout to close other 
                        FrontChannelLogoutUri = "https://localhost:44318/home/frontchannellogout",
                        BackChannelLogoutUri  = "https://localhost:44318/home/backchannellogout",
                    }
                })

                .AddInMemoryApiResources(new List<ApiResource>()
                {
                    new ApiResource()
                    {
                        Name = "sampleapi",
                        DisplayName = "This is display name for sample api",
                        Scopes = new Scope[]
                        {
                           new Scope()
                           {
                               Name = "sampleapi",
                               DisplayName = "This is display name for sample api scope",
                               UserClaims = {ClaimTypes.Email}
                           }
                        },
                        UserClaims = {}
                    }
                })
                .AddDeveloperSigningCredential(persistKey:true);
                //.AddSigningCredential(LoadFromStore("86a4a37c5083500ac53525ba7b84c1d5ffb363ad"))
                //.AddValidationKey(LoadFromStore("00a57dcf0cdabcfa2345454a93d8923eadd8a17e"));
                
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
        
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private X509Certificate2 LoadFromStore(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly:false);
                if (certificates.Count > 0)
                {
                    return certificates[0];
                }
                return null;
            }
        }
    }
}
