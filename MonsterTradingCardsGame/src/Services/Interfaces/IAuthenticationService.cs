using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services.Interfaces
{
    public interface IAuthenticationService
    {
        bool VerifyCredentials(string username, string password);
        bool ValidateToken(string token);
        string GenerateToken(User user);
        User? GetUserFromToken(string token);
    }
}
