using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using System.Text.Json;


namespace MonsterTradingCardsGame.Controllers
{

    public class TradingController
    {
        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;

        public TradingController(ICardRepository cardRepository, IUserRepository userRepository)
        {
            this._cardRepository = cardRepository;
            this._userRepository = userRepository;

        }

        // [Route("GET", "/tradings")]
        // public void getTradingDeals(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        // {
        //     try
        //     {
        //         User? user = httpEventArguments.User;

        //         if (user == null)
        //         {
        //             httpEventArguments.Reply(401, "You need to be logged in to see trading deals");
        //             return;
        //         }

        //         List<TradingDeal> tradingDeals = _cardRepository.GetTradingDeals();

        //         if (tradingDeals.Count == 0)
        //         {
        //             httpEventArguments.Reply(204, "There are no trading deals available");
        //             return;
        //         }

        //         httpEventArguments.Reply(JsonSerializer.Serialize(tradingDeals));
        //     }
        //     catch (Exception e)
        //     {
        //         httpEventArguments.Reply(500, e.Message);
        //     }
        // }

        // [Route("POST", "/tradings")]
        // public void createTradingDeal(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        // {
        //     try
        //     {
        //         User? user = httpEventArguments.User;

        //         if (user == null)
        //         {
        //             httpEventArguments.Reply(401, "You need to be logged in to create a trading deal");
        //             return;
        //         }

        //         TradingDeal tradingDeal = JsonSerializer.Deserialize<TradingDeal>(httpEventArguments.Payload);

        //         if (tradingDeal == null)
        //         {
        //             httpEventArguments.Reply(400, "Payload is not a valid trading deal");
        //             return;
        //         }


        //     }
        //     catch (Exception e)
        //     {
        //         httpEventArguments.Reply(500, e.Message);
        //     }
        // }
    }
}
