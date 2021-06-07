using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Client
{
    public class CookieEventHandler : CookieAuthenticationEvents
    {
        public CookieEventHandler(LogoutSessionManager logoutSessions)
        {
            LogoutSessions = logoutSessions;
        }

        public LogoutSessionManager LogoutSessions { get; }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            Console.WriteLine($">>>>>> Checking if user is authenticated");
            if (context.Principal.Identity.IsAuthenticated)
            {
                Console.WriteLine($">>>>>> user has valid cookie");
                var sub = context.Principal.FindFirst("sub")?.Value;
                var sid = context.Principal.FindFirst("sid")?.Value;

                if (LogoutSessions.IsLoggedOut(sub, sid))
                {
                    Console.WriteLine($">>>>>> but this cookie is logged out from Identity Server. Killing the cookie");
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync();

                    // todo: if we have a refresh token, it should be revoked here.
                }
            }
        }
    }
}