using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ICardRepository
    {
        List<Card> GetCardsByUser(User user);

        List<Card> GetDeckByUser(User user);

        Card? GetCardById(string cardId);

        CardType GetCardTypeFromName(string cardName);
        ElementType GetCardElementFromName(string cardName);

        public bool SaveCard(Card card);
        public bool SavePackage(List<Card> package);
        List<Card> GetCardPackage();
        public bool LockCardInTrade(User user, Card card);
        public bool UnlockCard(User user, Card card);
        bool SavePackageToUser(User user, List<Card> package);
        public bool CheckIfCardIsOwnedByUser(User user, Card card);
        public void ChangeCardOwner(User user, Card card);

    }
}
