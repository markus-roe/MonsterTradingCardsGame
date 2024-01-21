using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Utils.UserStats;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface IUserRepository
    {
        User? GetUserByUsername(string username);
        public User? GetUserById(int id);
        bool SetCardDeck(User user, List<Card> cards);
        public UserStats? GetStatsByUser(User user);
        public List<UserStats>? GetScoreboard();
        public bool AddWin(User user);
        public bool AddLoss(User user);
        public void UpdateElo(User user, int eloChange);
        int? SaveUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(User user);
        public bool SaveCardToUserDeck(User user, Card card);
        public bool RemoveCardFromUser(User user, Card card);
    }
}
