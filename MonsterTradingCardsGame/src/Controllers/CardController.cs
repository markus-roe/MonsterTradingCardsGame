using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Services.Interfaces;
using System.Text.Json;

namespace MonsterTradingCardsGame.Controllers
{
    public class CardController
    {
        private readonly ICardRepository cardRepository;

        public CardController(ICardRepository cardRepository, IAuthenticationService authService)
        {
            this.cardRepository = cardRepository;
        }

        [Route("GET", "/cards")]
        public void GetCardsByUser(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            User user = httpEventArguments.User;
            string username = user.Username;

            var cards = cardRepository.GetCardsByUsername(username);
            if (cards.Count == 0)
            {
                httpEventArguments.Reply(204);
                return;
            }

            var response = JsonSerializer.Serialize(cards);
            httpEventArguments.Reply(200, response);
        }


    }
}
