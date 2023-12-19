using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface ITradingRepository
    {
        public List<TradingDeal>? GetTradingDeals();
        public TradingDeal? GetTradingDealById(string id);
        public TradingDeal? createTradingDeal(TradingDeal tradingDeal, User user);

        public int GetTradingDealUserId(TradingDeal tradingDeal);

        public bool DeleteTradingDeal(TradingDeal tradingDeal);

    }
}
