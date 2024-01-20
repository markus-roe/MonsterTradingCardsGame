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
            _tradingRepository = tradingRepository;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method retrieves all available trading deals. </summary>
        [Route("GET", "/tradings")]
        public void GetTradingDeals(IHttpServerEventArguments httpEventArguments)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method creates a new trading deal. </summary>
        [Route("POST", "/tradings")]
        public void CreateTradingDeal(IHttpServerEventArguments httpEventArguments)
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
                bool cardIsLockedInDeck = false;

                foreach (Card card in user.Deck)
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
                TradingDeal? createdTradingDeal = _tradingRepository.CreateTradingDeal(tradingDeal, user);

                if (createdTradingDeal == null)
                {
                    httpEventArguments.Reply(500, "The trading deal could not be created.");
                    return;
                }

                //mark card as locked by trade
                bool cardLocked = _cardRepository.LockCardInTrade(user, tradedCard);

                if (cardLocked == false)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method deletes a specific trading deal. </summary>
        [Route("DELETE", "/tradings/:tradingdealid")]
        public void DeleteTradingDeal(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                User? user = httpEventArguments.User;

                string tradingDealId = httpEventArguments.Parameters["tradingdealid"];

                TradingDeal? tradingDeal = _tradingRepository.GetTradingDealById(tradingDealId);

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method executes a trading deal. </summary>
        [Route("POST", "/tradings/:tradingdealid")]
        public void ExecuteTradingDeal(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                User? user = httpEventArguments.User;

                string tradingDealId = httpEventArguments.Parameters["tradingdealid"];

                //get trading deal
                TradingDeal? tradingDeal = _tradingRepository.GetTradingDealById(tradingDealId);

                if (tradingDeal == null)
                {
                    httpEventArguments.Reply(404, "The trading deal does not exist!");
                    return;
                }

                //get user from trading deal
                int tradingDealUserId = _tradingRepository.GetTradingDealUserId(tradingDeal);

                if (tradingDealUserId == user.Id)
                {
                    httpEventArguments.Reply(403, "Trading with self is not allowed!");
                    return;
                }

                User? tradingUser = _userRepository.GetUserById(tradingDealUserId);


                //get card from trading deal
                Card? tradingDealCard = _cardRepository.GetCardById(tradingDeal.CardToTrade);

                if (tradingDealCard == null)
                {
                    httpEventArguments.Reply(400, "Card does not exist!");
                    return;
                }

                //body in http request = 2c98cd06-518b-464c-b911-8d787216cddd

                //get offered card from payload
                string? offeredCardId = JsonSerializer.Deserialize<string>(httpEventArguments.Payload);

                if (string.IsNullOrEmpty(offeredCardId))
                {
                    httpEventArguments.Reply(400, "Card does not exist!");
                    return;
                }

                Card? offeredCard = _cardRepository.GetCardById(offeredCardId);

                if (offeredCard == null)
                {
                    httpEventArguments.Reply(400, "Card does not exist!");
                    return;
                }

                //check if card is owned by user
                bool cardIsOwnedByUser = _cardRepository.CheckIfCardIsOwnedByUser(user, offeredCard);

                if (cardIsOwnedByUser == false)
                {
                    httpEventArguments.Reply(403, "The card is not owned by the user!");
                    return;
                }

                //check if card is locked in deck
                bool cardIsLockedInDeck = user.Deck.Any(card => card.Id == offeredCard.Id);


                if (cardIsLockedInDeck)
                {
                    httpEventArguments.Reply(403, "The card is locked in the deck!");
                    return;
                }

                //check if requirements of trade are met
                if (offeredCard.Damage < tradingDeal.MinimumDamage || offeredCard.Type.ToString() != tradingDeal.Type)
                {
                    httpEventArguments.Reply(403, "The card does not meet the requirements of the trade!");
                    return;
                }

                // -----------------
                // EXECUTE TRADE

                _cardRepository.ChangeCardOwner(tradingUser, offeredCard);
                _cardRepository.ChangeCardOwner(user, tradingDealCard);

                _cardRepository.UnlockCard(user, tradingDealCard);

                _tradingRepository.DeleteTradingDeal(tradingDeal);

                httpEventArguments.Reply(200, "The trade was successfully executed.");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ExecuteTradingDeal: " + e.Message);
                httpEventArguments.Reply(500, e.Message);
            }
        }
    }
}

