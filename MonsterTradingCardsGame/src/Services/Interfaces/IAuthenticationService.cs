using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services.Interfaces
{
    public interface IAuthenticationService
    {
        bool VerifyCredentials(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        bool ValidateToken(string token);
        string GenerateToken(User user);
        User? GetUserFromToken(string token);
    }
}
