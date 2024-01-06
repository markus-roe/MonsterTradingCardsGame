using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using System.Text.Json;

namespace MonsterTradingCardsGame.Controllers
{
  public class CardController
  {
    private readonly ICardRepository _cardRepository;
    private readonly IUserRepository _userRepository;

    public CardController(ICardRepository cardRepository, IUserRepository userRepository)
    {
      _cardRepository = cardRepository;
      _userRepository = userRepository;
    }


    [Route("POST", "/packages")]
    public void CreatePackage(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User? user = httpEventArguments.User;


        if (user != null && user.Username != "admin")
        {
          httpEventArguments.Reply(403, "You are not allowed to create packages");
          return;
        }

        if (string.IsNullOrWhiteSpace(httpEventArguments.Payload))
        {
          httpEventArguments.Reply(400, "Payload is not a valid list of cards");
          return;
        }

        //check if payload is valid json
        try
        {
          var obj = JsonSerializer.Deserialize<object>(httpEventArguments.Payload.Trim());
        }
        catch (JsonException)
        {
          httpEventArguments.Reply(400, "Payload is not a valid list of cards");
          return;
        }



        List<Card>? package = JsonSerializer.Deserialize<List<Card>>(httpEventArguments.Payload);

        if (package == null)
        {
          httpEventArguments.Reply(400, "Payload is not a valid list of cards");
          return;
        }

        //set type and element of card, since its not sent by admin (?)
        foreach (Card card in package)
        {
          card.Type = _cardRepository.GetCardTypeFromName(card.Name);
          card.Element = _cardRepository.GetCardElementFromName(card.Name);
        }

        if (package.Count != 5)
        {
          httpEventArguments.Reply(400, "You need to provide 5 cards");
          return;
        }

        foreach (Card card in package)
        {
          if (_cardRepository.GetCardById(card.Id) != null)
          {
            httpEventArguments.Reply(409, "At least one card in the packages already exists");
            return;
          }
        }


        if (_cardRepository.SavePackage(package) == null)
        {
          httpEventArguments.Reply(500, "Internal server error: Could not save package");
          return;
        }

        httpEventArguments.Reply(200, "Package and cards successfully created");
      }
      catch (Exception ex)
      {
        httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
      }
    }

    [Route("GET", "/cards")]
    public void GetCardsByUser(IHttpServerEventArguments httpEventArguments)
    {
      User user = httpEventArguments.User;

      var response = JsonSerializer.Serialize(user.Stack);
      httpEventArguments.Reply(200, response);
    }

    [Route("GET", "/deck")]
    public void GetDeck(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User user = httpEventArguments.User;
        var deck = user.Deck;

        if (deck.Count == 0)
        {
          httpEventArguments.Reply(204, "The request was fine, but the deck doesn't have any cards");
          return;
        }

        string format = "json";

        if (httpEventArguments?.QueryParameters?.ContainsKey("format") == true)
        {
          format = httpEventArguments.QueryParameters["format"].ToLower();
        }

        string response;
        if (format == "plain")
        {
          response = string.Join("\n", deck.Select(card => card.ToString()));
        }
        else
        {
          response = JsonSerializer.Serialize(deck);
        }

        if (httpEventArguments != null) //Idk why this is needed, but it is getting rid of the null warning...? only method where this appears
        {
          httpEventArguments.Reply(200, response);
        }
      }
      catch (Exception ex)
      {
        httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
      }
    }

    [Route("PUT", "/deck")]
    public void ConfigureDeck(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User user = httpEventArguments.User;

        // Check if the request body is empty
        if (string.IsNullOrEmpty(httpEventArguments.Payload))
        {
          httpEventArguments.Reply(400, "The provided deck did not include the required amount of cards");
          return;
        }

        //check if payload is valid json
        try
        {
          var obj = JsonSerializer.Deserialize<object>(httpEventArguments.Payload.Trim());
        }
        catch (JsonException)
        {
          httpEventArguments.Reply(400, "The provided deck did not include the required amount of cards");
          return;
        }

        // Deserialize the request body
        var cardIds = JsonSerializer.Deserialize<string[]>(httpEventArguments.Payload);

        // Check if the request body is valid
        if (cardIds == null || cardIds.Length != 4)
        {
          httpEventArguments.Reply(400, "The provided deck did not include the required amount of cards");
          return;
        }

        // Check if the user owns all the cards and they are not locked
        var cards = user.Stack.Where(card => cardIds.Contains(card.Id) && !card.IsLocked);
        if (cards.Count() != 4)
        {
          httpEventArguments.Reply(403, "At least one of the provided cards does not belong to the user or is not available.");
          return;
        }

        // Configure the deck
        _userRepository.SetCardDeck(user, cards.ToList());

        httpEventArguments.Reply(200, "The deck has been successfully configured.");
      }
      catch (Exception ex)
      {
        httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
      }
    }

    [Route("POST", "transactions/packages")]
    public void BuyPackage(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User user = httpEventArguments.User;

        if (user.Coins < 5)
        {
          httpEventArguments.Reply(403, "Not enough money for buying a card package");
          return;
        }

        List<Card> package = _cardRepository.GetCardPackage();

        if (package.Count == 0)
        {
          httpEventArguments.Reply(404, "No card package available for buying");
          return;
        }


        bool successfullySaved = _cardRepository.SavePackageToUser(user, package);

        if (successfullySaved == false)
        {
          httpEventArguments.Reply(500, "Internal server error: Could not save package to user");
          return;
        }

        user.Coins -= 5;
        _userRepository.UpdateUser(user);

        httpEventArguments.Reply(200, "Package and cards successfully bought");
      }
      catch (Exception ex)
      {
        httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
      }
    }


  }
}
