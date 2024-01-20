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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method is used to start a battle. It calls the EnterLobbyAsync method from the LobbyService class. </summary>
        [Route("POST", "/battles")]
        public async void StartBattle(IHttpServerEventArguments httpEventArguments)
        {
            var user = httpEventArguments.User;

            string battleResult = await lobbyService.EnterLobbyAsync(user);
            httpEventArguments.Reply(200, battleResult);
        }
    }
}
