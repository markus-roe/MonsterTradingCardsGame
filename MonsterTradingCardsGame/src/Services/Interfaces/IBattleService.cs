using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services.Interfaces
{
    public interface IBattleService
    {
        public string StartBattle(User user1, User user2);
        public bool AdjustDamageBasedOnElement(Card card, ElementType opponentElement);
        public Card? CompareCardDamage(Card card1, Card card2);
    }
}
