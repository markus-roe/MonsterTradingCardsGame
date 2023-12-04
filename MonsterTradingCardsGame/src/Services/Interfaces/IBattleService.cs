using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services.Interfaces
{
    public interface IBattleService
    {
        public string StartBattle(User user1, User user2);
    }
}
