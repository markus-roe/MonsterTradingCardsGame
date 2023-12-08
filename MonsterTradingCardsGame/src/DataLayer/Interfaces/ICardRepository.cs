using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ICardRepository : IRepository<Card>
    {
        List<Card> GetCardsByUsername(string username);

        List<Card> GetDeckByUsername(string username);

        Card GetCardById(int cardId);

        public void Save(Card card);
    }
}
