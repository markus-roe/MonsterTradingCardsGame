using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository userRepository;

        public AuthenticationService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public bool VerifyCredentials(string username, string password)
        {
            var user = userRepository.GetUserByUsername(username);
            if (user != null)
            {
                return password == user.Password;
            }

            return false;
        }

        public string GenerateToken(User user)
        {
            return $"{user.Username}-mtcgToken";
        }
    }
}
