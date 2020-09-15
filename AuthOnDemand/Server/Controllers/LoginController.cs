using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthOnDemand.Server.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// Triggers authentication challenge with Google IDP
        /// </summary>
        [HttpGet("challenge")]
        public IActionResult Challenge([FromQuery] string returnUrl)
        {
            // Check if return url is valid / not malicious
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "~/";
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                throw new Exception("invalid return URL");
            }

            // Send challenge to Google Oauth server
            var props = new AuthenticationProperties
            {
                // After authentication call this handler
                RedirectUri = "/Login/callback",

                // Stuff you need to use later
                // Those values are stored in a cookie, so don't add what you won't need
                Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", "Google" }
                    }
            };


            // Start authentication flow
            return Challenge(props, "Google");
        }

        /// <summary>
        /// Process user login after response from Google IDP
        /// </summary>
        [HttpGet("callback")]
        public async Task<ActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (result?.Succeeded != true)
            {
                // Unexpected problem
                throw new Exception();
            }

            // For this to work you need to request any additional scope 
            // Otherwise you only gets an identity token
            // In startup cs: options.Scope.Add("https://www.googleapis.com/auth/gmail.labels");
            var token = result.Properties.Items[".Token.access_token"];
            Console.WriteLine(token);

            var expires = DateTime.Parse(result.Properties.Items[".Token.expires_at"]);
            Console.WriteLine(expires);


            // If you requested a refresh token you can extract it now
            if (result.Properties.Items.ContainsKey(".Token.refresh_token"))
            {
                // You only get it once, to request another you need to unlink
                // the app from https://myaccount.google.com/permissions
                var refreshToken = result.Properties.Items[".Token.refresh_token"];
                Console.WriteLine(refreshToken);
            }

            // Optional - If you need to further customize identity
            // 1. Logout from the Google schema
            await HttpContext.SignOutAsync();

            // 2. Create a claim list
            var claims = new List<Claim>();
            foreach (var claim in result.Principal.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            claims.Add(new Claim("auth", "myauth"));

            // 3. Create a new identity to store in the cookie
            var identity = new ClaimsIdentity(claims, "mycustomauth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);


            // Send the user to the return url
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
            return LocalRedirect(returnUrl);
        }
    }
}
