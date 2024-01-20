
namespace MonsterTradingCardsGame.Services.Interfaces;

public interface IDatabaseInitializationService
{
    public Task InitializeOrResetDatabase(bool resetDB);

}
