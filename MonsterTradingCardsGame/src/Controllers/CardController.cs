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

        // CardController currently not used
    }
}
