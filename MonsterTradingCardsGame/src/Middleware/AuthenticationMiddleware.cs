using MonsterTradingCardsGame.Server;

namespace MonsterTradingCardsGame.Middleware
{
    public class AuthenticationMiddleware : IMiddleware
    {
        public void Invoke(HttpServerEventArguments httpEventArguments)
        {
            // Check if the Authorization header is present
            var authHeader = httpEventArguments.Headers
                .FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            // Check if the Authorization header is correctly formatted as a Bearer token
            if (authHeader == null || !authHeader.Value.StartsWith("Bearer "))
            {
                httpEventArguments.Reply(401, "Unauthorized");
                return;
            }

            // Extract the token from the Authorization header
            var token = authHeader.Value.Substring("Bearer ".Length).Trim();

            // TODO: Implement your token validation logic here
            bool isAuthenticated = ValidateToken(token);

            if (!isAuthenticated)
            {
                httpEventArguments.Reply(401, "Unauthorized");
                return;
            }

        }

        private bool ValidateToken(string token)
        {
            // TODO: Add token validation logic 
            return true;
        }
    }
}
