using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ICardRepository : IRepository<Card>
    {
        List<Card> GetCardsByUser(User user);

        List<Card> GetDeckByUser(User user);

        Card GetCardById(string cardId);

        CardType GetCardTypeFromName(string cardName);
        ElementType GetCardElementFromName(string cardName);

        public void Save(Card card);
        public bool SavePackage(User user, List<Card> package);

    }
}
