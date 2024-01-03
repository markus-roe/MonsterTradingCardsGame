using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ISessionRepository
    {
        bool AddSession(Session session);
        Session? GetSessionById(string sessionId);
        bool RemoveSession(Session session);
    }
}
