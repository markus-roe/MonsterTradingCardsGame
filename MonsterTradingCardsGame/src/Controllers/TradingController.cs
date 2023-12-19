using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using System.Text.Json;


namespace MonsterTradingCardsGame.Controllers
{

    public class TradingController
    {
        private readonly ITradingRepository _tradingRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;

        public TradingController(ITradingRepository tradingRepository, IUserRepository userRepository, ICardRepository cardRepository)
        {
            this._tradingRepository = tradingRepository;
            this._userRepository = userRepository;
            this._cardRepository = cardRepository;
        }

        [Route("GET", "/tradings")]
        public void getTradingDeals(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User? user = httpEventArguments.User;

                List<TradingDeal>? tradingDeals = _tradingRepository.GetTradingDeals();

                if (tradingDeals == null || tradingDeals.Count == 0)
                {
                    httpEventArguments.Reply(204, "There are no trading deals available");
                    return;
                }

                httpEventArguments.Reply(200, JsonSerializer.Serialize(tradingDeals));
            }
            catch (Exception e)
            {
                httpEventArguments.Reply(500, e.Message);
            }
        }

        [Route("POST", "/tradings")]
        public void createTradingDeal(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User? user = httpEventArguments.User;

                TradingDeal? tradingDeal = JsonSerializer.Deserialize<TradingDeal>(httpEventArguments.Payload);

                if (tradingDeal == null)
                {
                    httpEventArguments.Reply(400, "The trading deal could not be created. Please check your input.");
                    return;
                }

                //check if card in trading deal exists
                Card? tradedCard = _cardRepository.GetCardById(tradingDeal.CardToTrade);

                if (tradedCard == null)
                {
                    httpEventArguments.Reply(400, "Card does not exist!");
                    return;
                }

                //check if card is owned by user
                List<Card>? userCards = _cardRepository.GetCardsByUser(user);

                if (userCards == null)
                {
                    httpEventArguments.Reply(400, "User does not exist!");
                    return;
                }

                bool cardIsOwnedByUser = false;

                foreach (Card card in userCards)
                {
                    if (card.Id == tradingDeal.CardToTrade)
                    {
                        cardIsOwnedByUser = true;
                    }
                }

                if (cardIsOwnedByUser == false)
                {
                    httpEventArguments.Reply(403, "The card is not owned by the user!");
                    return;
                }

                //check if card is locked in deck
                List<Card>? userDeck = _cardRepository.GetDeckByUser(user);

                if (userDeck == null)
                {
                    httpEventArguments.Reply(400, "User does not exist!");
                    return;
                }

                bool cardIsLockedInDeck = false;

                foreach (Card card in userDeck)
                {
                    if (card.Id == tradingDeal.CardToTrade)
                    {
                        cardIsLockedInDeck = true;
                    }
                }

                if (cardIsLockedInDeck == true)
                {
                    httpEventArguments.Reply(403, "The card is locked in the deck!");
                    return;
                }

                //check if trading deal already exists
                TradingDeal? existingTradingdeal = _tradingRepository.GetTradingDealById(tradingDeal.Id);

                if (existingTradingdeal != null)
                {
                    httpEventArguments.Reply(409, "A trading deal with this ID already exists!");
                    return;
                }

                //create trading deal
                TradingDeal? createdTradingDeal = _tradingRepository.createTradingDeal(tradingDeal, user);

                if (createdTradingDeal == null)
                {
                    httpEventArguments.Reply(500, "The trading deal could not be created.");
                    return;
                }

                httpEventArguments.Reply(201, "The trading deal was successfully created.");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error  in createTradingDeal: " + e.Message);
                httpEventArguments.Reply(500, e.Message);
            }
        }

        [Route("DELETE", "/tradings/:id")]
        public void deleteTradingDeal(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User? user = httpEventArguments.User;

                string id = parameters["id"];

                TradingDeal? tradingDeal = _tradingRepository.GetTradingDealById(id);

                if (tradingDeal == null)
                {
                    httpEventArguments.Reply(404, "The trading deal does not exist!");
                    return;
                }

                //GetTradingDealUserId
                int tradingDealUserId = _tradingRepository.GetTradingDealUserId(tradingDeal);

                if (tradingDealUserId == -1)
                {
                    httpEventArguments.Reply(500, "The trading deal could not be deleted!");
                    return;
                }

                if (tradingDealUserId != user.Id)
                {
                    httpEventArguments.Reply(403, "The trading deal is not owned by the user!");
                    return;
                }

                //delete trading deal
                bool deleted = _tradingRepository.DeleteTradingDeal(tradingDeal);

                if (deleted == false)
                {
                    httpEventArguments.Reply(500, "The trading deal could not be deleted!");
                    return;
                }

                httpEventArguments.Reply(200, "The trading deal was successfully deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in deleteTradingDeal: " + e.Message);
                httpEventArguments.Reply(500, e.Message);
            }
        }
    }
}
