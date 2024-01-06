using MonsterTradingCardsGame.Server;
namespace MonsterTradingCardsGame.Controllers
{
    public class BattleController
    {
        private readonly LobbyService lobbyService;

        public BattleController(LobbyService lobbyService)
        {
            this.lobbyService = lobbyService;
        }

        [Route("POST", "/battles")]
        public async void StartBattle(IHttpServerEventArguments httpEventArguments)

        {

            var user = httpEventArguments.User;

            string battleResult = await lobbyService.EnterLobbyAsync(user);
            httpEventArguments.Reply(200, battleResult);
        }
    }
}
