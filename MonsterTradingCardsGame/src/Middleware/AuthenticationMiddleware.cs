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
            // Existing logic for bypassing token validation for login and registration

            // Extract the token from the Authorization header
            var token = ExtractToken(httpEventArguments);
            if (string.IsNullOrEmpty(token))
            {
                httpEventArguments.Reply(401, "Unauthorized");
                return;
            }

            bool isAuthenticated = authService.ValidateToken(token);
            if (!isAuthenticated)
            {
                httpEventArguments.Reply(401, "Unauthorized");
                return;
            }

            User user = authService.GetUserFromToken(token);
            httpEventArguments.User = user;
        }

        private string ExtractToken(HttpServerEventArguments httpEventArguments)
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
