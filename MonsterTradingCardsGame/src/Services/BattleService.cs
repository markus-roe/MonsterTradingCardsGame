using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;

public class BattleService : IBattleService
{
    public string StartBattle(User user1, User user2)
    {

        return $"{user1.Username} vs. {user2.Username}";
    }   
}
