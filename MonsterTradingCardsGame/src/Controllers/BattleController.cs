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
        public async void StartBattle(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {

            var user = httpEventArguments.User;

            string battleResult = await lobbyService.EnterLobbyAsync(user);
            httpEventArguments.Reply(200, battleResult);
        }
    }
}
