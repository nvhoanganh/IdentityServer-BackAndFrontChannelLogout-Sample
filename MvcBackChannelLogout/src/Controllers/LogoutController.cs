﻿using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class LogoutController : Controller
    {
        public LogoutSessionManager LogoutSessions { get; }

        public LogoutController(LogoutSessionManager logoutSessions)
        {
            LogoutSessions = logoutSessions;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string logout_token)
        {
            Console.WriteLine($"Received logout request with token via backchannel: {logout_token}");
            
            Response.Headers.Add("Cache-Control", "no-cache, no-store");
            Response.Headers.Add("Pragma", "no-cache");

            try
            {
                var user = await ValidateLogoutToken(logout_token);

                // these are the sub & sid to signout
                var sub = user.FindFirst("sub")?.Value;
                var sid = user.FindFirst("sid")?.Value;

                LogoutSessions.Add(sub, sid);

                return Ok();
            }
            catch { }

            return BadRequest();
        }

        private async Task<ClaimsPrincipal> ValidateLogoutToken(string logoutToken)
        {
            // https://ashend.medium.com/openid-connect-backchannel-logout-144a3198d2a
            var claims = await ValidateJwt(logoutToken);

            if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null) throw new Exception("Invalid logout token");

            var nonce = claims.FindFirstValue("nonce");
            if (!String.IsNullOrWhiteSpace(nonce)) throw new Exception("Invalid logout token");
            Console.WriteLine($"nonce is: {nonce}");

            var eventsJson = claims.FindFirst("events")?.Value;
            if (String.IsNullOrWhiteSpace(eventsJson)) throw new Exception("Invalid logout token");
            Console.WriteLine($"eventsJson is: {eventsJson}");

            var events = JObject.Parse(eventsJson);
            var logoutEvent = events.TryGetValue("http://schemas.openid.net/event/backchannel-logout");
            if (logoutEvent == null) throw new Exception("Invalid logout token");

            return claims;
        }

        private static async Task<ClaimsPrincipal> ValidateJwt(string jwt)
        {
            // read discovery document to find issuer and key material
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");

            var keys = new List<SecurityKey>();
            foreach (var webKey in disco.KeySet.Keys)
            {
                var key = new JsonWebKey()
                {
                    Kty = webKey.Kty,
                    Alg = webKey.Alg,
                    Kid = webKey.Kid,
                    X = webKey.X,
                    Y = webKey.Y,
                    Crv = webKey.Crv,
                    E = webKey.E,
                    N = webKey.N,
                };
                keys.Add(key);
            }

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = disco.Issuer,
                ValidAudience = "mvc.backchannel.sample",
                IssuerSigningKeys = keys,

                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role
            };

            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();

            var user = handler.ValidateToken(jwt, parameters, out var _);
            return user;
        }
    }
}
