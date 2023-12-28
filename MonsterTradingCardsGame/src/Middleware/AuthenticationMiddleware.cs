using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Middleware
{
    public class AuthenticationMiddleware : IMiddleware
    {
        private readonly IAuthenticationService authService;

        public AuthenticationMiddleware(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        public void Invoke(HttpServerEventArguments httpEventArguments)
        {
            // Bypass token validation for specific login and registration requests
            if (httpEventArguments.Method == "POST" && httpEventArguments.Path.Equals("/users") || httpEventArguments.Path.Equals("/sessions"))
            {
                return;
            }

            // Extract the token from the Authorization header
            string? token = ExtractToken(httpEventArguments);

            if (string.IsNullOrEmpty(token))
            {
                httpEventArguments.Reply(401, "Access token is missing or invalid");
                return;
            }

            if (authService.ValidateToken(token) == false)
            {
                httpEventArguments.Reply(401, "Access token is missing or invalid");
                return;
            }

            User? user = authService.GetUserFromToken(token);

            if (user == null)
            {
                httpEventArguments.Reply(401, "Access token is missing or invalid");
                return;
            }
            httpEventArguments.User = user;
        }

        private string? ExtractToken(HttpServerEventArguments httpEventArguments)
        {
            var authHeader = httpEventArguments.Headers
                .FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            if (authHeader == null || !authHeader.Value.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader.Value.Substring("Bearer ".Length).Trim();
        }
    }
}
