using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services.Interfaces
{
    public interface IAuthenticationService
    {
        bool VerifyCredentials(string username, string password);
        string GenerateToken(User user);
    }
}
