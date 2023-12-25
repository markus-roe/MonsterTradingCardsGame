
namespace MonsterTradingCardsGame.Utils.UserStats
{
    public class UserStats
    {
        public required string Name { get; set; }
        public required int Elo { get; set; }
        public required int Wins { get; set; }
        public required int Losses { get; set; }
    }
}