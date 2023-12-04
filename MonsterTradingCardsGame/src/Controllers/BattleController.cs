using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class BattleController
    {
        private readonly LobbyService lobbyService;
        private readonly IAuthenticationService authenticationService;

        public BattleController(LobbyService lobbyService, IAuthenticationService authenticationService)
        {
            this.lobbyService = lobbyService;
            this.authenticationService = authenticationService;
        }

        [Route("POST", "/battles")]
        public async void StartBattle(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            // Extracting token from the 'Authorization' header
            if (!httpEventArguments.Headers.Any(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                httpEventArguments.Reply(401, "Unauthorized: No Authorization header.");
                return;
            }

            var authHeader = httpEventArguments.Headers.First(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)).Value;
            if (!authHeader.StartsWith("Bearer "))
            {
                httpEventArguments.Reply(401, "Unauthorized: Invalid Authorization header format.");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var user = authenticationService.GetUserFromToken(token);
            if (user == null)
            {
                httpEventArguments.Reply(401, "Unauthorized: Invalid token.");
                return;
            }

            string battleResult = await lobbyService.EnterLobbyAsync(user);
            httpEventArguments.Reply(200, battleResult);
        }
    }
}
