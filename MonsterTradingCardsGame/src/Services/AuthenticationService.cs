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

        public User GetUserFromToken(string token)
        {
            return userRepository.GetUserByUsername(token.Split("-")[0]);
        }



        public bool ValidateToken(string token)
        {

            User user = userRepository.GetUserByUsername(token.Split("-")[0]);
            if (user == null)
            {
                return false;
            }

            return true;
        }

        public string getUsernameFromHeader(string token)
        {
            return token.Split("-")[0];
        }

        public string GenerateToken(User user)
        {
            return $"{user.Username}-mtcgToken";
        }
    }
}
