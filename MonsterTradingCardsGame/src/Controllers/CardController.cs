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
      this._cardRepository = cardRepository;
      this._userRepository = userRepository;
    }


    [Route("POST", "/packages")]
    public void createPackage(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User? user = httpEventArguments.User;


        if (user != null && user.Username != "admin")
        {
          httpEventArguments.Reply(403, "You are not allowed to create packages");
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


        if (_cardRepository.SavePackage(package) == false)
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

        // Extract the format parameter from the query parameters
        httpEventArguments.QueryParameters.TryGetValue("format", out var format);
        format = format?.ToLower() ?? "json";

        string response;
        if (format == "plain")
        {
          response = string.Join("\n", deck.Select(card => card.ToString()));
        }
        else
        {
          response = JsonSerializer.Serialize(deck);
        }

        httpEventArguments.Reply(200, response);
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
          httpEventArguments.Reply(400, "Bad Request: Request body is missing or empty.");
          return;
        }

        // Deserialize the request body
        var cardIds = JsonSerializer.Deserialize<string[]>(httpEventArguments.Payload);

        // Check if the request body is valid
        if (cardIds == null || cardIds.Length != 4)
        {
          httpEventArguments.Reply(400, "Bad Request: Request body is invalid.");
          return;
        }

        // Check if the user owns all the cards
        var cards = user.Stack.Where(card => cardIds.Contains(card.Id));
        if (cards.Count() != 4)
        {
          httpEventArguments.Reply(403, "Forbidden: At least one of the provided cards does not belong to the user or is not available.");
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
    public void buyPackage(IHttpServerEventArguments httpEventArguments)
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


        _cardRepository.SavePackageToUser(user, package);
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
