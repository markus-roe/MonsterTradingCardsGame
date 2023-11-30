using MonsterTradingCardsGame.Server;

namespace MonsterTradingCardsGame.Middleware
{
    public class AuthenticationMiddleware : IMiddleware
    {
        public void Invoke(HttpServerEventArguments httpEventArguments)
        {
            // Bypass token validation for specific login and registration requests
            if ((httpEventArguments.Method == "POST" && httpEventArguments.Path.Equals("/users") || httpEventArguments.Path.Equals("/sessions")))
            {
                return;
            }

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

            // Implement your token validation logic here
            bool isAuthenticated = ValidateToken(token);

            if (!isAuthenticated)
            {
                httpEventArguments.Reply(401, "Unauthorized");
                return;
            }
        }

        private bool ValidateToken(string token)
        {
            //TODO: Add token validation logic 
            return true; // Placeholder for actual validation logic
        }
    }
}
