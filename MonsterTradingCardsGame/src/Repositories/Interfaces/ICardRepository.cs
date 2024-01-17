using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ICardRepository
    {
        List<Card> GetCardsByUser(User user);

        List<Card> GetDeckByUser(User user);

        Card? GetCardById(string cardId); //tested

        CardType GetCardTypeFromName(string cardName); //tested
        ElementType GetCardElementFromName(string cardName); //tested
        public int? SavePackage(List<Card> package); //tested
        public bool SaveCard(Card card);

        public bool DeletePackageById(int packageId); //tested

        List<Card> GetCardPackage(); //tested
        public bool LockCardInTrade(User user, Card card);
        public bool UnlockCard(User user, Card card);
        bool SavePackageToUser(User user, List<Card> package);
        public bool CheckIfCardIsOwnedByUser(User user, Card card);
        public void ChangeCardOwner(User user, Card card);


    }
}
