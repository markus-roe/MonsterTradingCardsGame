
namespace MonsterTradingCardsGame.Models
{
    public class TradingDeal
    {
        public required string Id { get; set; }
        public required string CardToTrade { get; set; }
        public required string Type { get; set; }
        public required float MinimumDamage { get; set; }
    }
}