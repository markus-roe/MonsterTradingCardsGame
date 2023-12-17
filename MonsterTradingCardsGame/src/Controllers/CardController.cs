using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using System.Text.Json;

namespace MonsterTradingCardsGame.Controllers
{
  public class CardController
  {
    private readonly ICardRepository _cardRepository;

    public CardController(ICardRepository cardRepository)
    {
      this._cardRepository = cardRepository;
    }


    [Route("POST", "/packages")]
    public void createPackage(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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
  }
}
