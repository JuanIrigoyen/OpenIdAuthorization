using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcClient.Models;using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace MvcClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Privacy()
        {
            var access_token = await HttpContext.GetTokenAsync("access_token");
            var id_token = await HttpContext.GetTokenAsync("id_token");
            return View(new TokensModel() { AccessToken = access_token, IdToken = id_token });
        }

        [Authorize]
        public IActionResult Logout()
        {
            return new SignOutResult()
            {
                AuthenticationSchemes = new List<string>()
                {
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme
                }
            };
        }

        [HttpGet]
        public IActionResult FrontChannelLogout(string id, string iss)
        {
            return new SignOutResult()
            {
                AuthenticationSchemes = new List<string>()
                {
                    CookieAuthenticationDefaults.AuthenticationScheme
                }
            };
        }

        [HttpPost]
        public IActionResult BackChannelLogout(string logout_token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var data = tokenHandler.ReadJwtToken(logout_token);
          
             //var isValid = 
            var subject = data.Claims.Where(c => c.Type == JwtClaimTypes.Subject)
                              .SingleOrDefault();
            return Ok();


            return new SignOutResult()
            {
                AuthenticationSchemes = new List<string>()
                {
                    CookieAuthenticationDefaults.AuthenticationScheme
                }
            };
        }

        private async Task<bool> IsValid(string logout_token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var httpClient = new HttpClient();
            var document = await httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest()
            {
                Address = "https://locahost:4438"
            });

            var keys = new List<SecurityKey>();

            foreach (var item in document.KeySet.Keys)
            {
                var exponent = Base64Url.Decode(item.E);
                var modulo = Base64Url.Decode(item.N);

                var key = new RsaSecurityKey(new RSAParameters()
                {
                    Modulus = modulo,
                    Exponent = exponent
                });

                keys.Add(key);
            }

            var tokenValidtationParameters = new TokenValidationParameters()
            {
                ValidIssuer = document.Issuer,
                ValidAudience = "hybrid-sample",
                IssuerSigningKeys = keys
            };

            var principal = tokenHandler.ValidateToken(logout_token, tokenValidtationParameters, out _);
            return principal != null;
        }


        [Authorize]
        public async Task<IActionResult> WheatherForecast()
        {
            using (var httpClient = new HttpClient())
            {
                var access_token = await HttpContext.GetTokenAsync("access_token");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");

                var response = await httpClient.GetAsync("https://localhost:44331/WeatherForecast");
                if (response.IsSuccessStatusCode)
                { 
                    return Redirect("Privacy");
                }
                else return Redirect("Index");
            }
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
