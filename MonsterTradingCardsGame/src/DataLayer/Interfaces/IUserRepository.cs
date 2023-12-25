using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Utils.UserStats;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetUserByUsername(string username);
        public User? GetUserById(int id);
        bool SetCardDeck(User user, List<Card> cards);
        public string? GetStatsByUser(User user);
        public List<UserStats>? GetScoreboard();
        public bool AddWin(User user);
        public bool AddLoss(User user);
        bool Save(User user);

    }
}
