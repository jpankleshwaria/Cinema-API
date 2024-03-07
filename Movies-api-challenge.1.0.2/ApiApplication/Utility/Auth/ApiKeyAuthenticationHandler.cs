using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ApiApplication.Utility.Auth
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyValues))
            {
                return AuthenticateResult.Fail("API key is missing in the request headers.");
            }

            string apiKey = apiKeyValues.FirstOrDefault();

            // Validate the API key (you will need to implement this logic)

            if (string.IsNullOrEmpty(apiKey) || !IsValidApiKey(apiKey))
            {
                return AuthenticateResult.Fail("Invalid API key." + Request.HttpContext.Request.Path);
            }

            // You can retrieve additional information about the user or claims here if needed
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "API user")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private bool IsValidApiKey(string apiKey)
        {
            // Implement logic to validate the API key as of now it's fixed api key
            return apiKey == "68e5fbda-9ec9-4858-97b2-4a8349764c63";
        }
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ApiKeyHeaderName { get; set; } = "X-Apikey";
    }
}
