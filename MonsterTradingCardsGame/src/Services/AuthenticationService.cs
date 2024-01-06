using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace MonsterTradingCardsGame.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly string secret;
        private bool isTesting;

        public AuthenticationService(IUserRepository userRepository, ISessionRepository sessionRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            bool.TryParse(configuration["IsTesting"], out isTesting);
            secret = configuration.GetSection("SecretKey")?.Value ?? string.Empty;
        }

        public bool VerifyCredentials(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user != null)
            {
                return password == user.Password;
            }

            return false;
        }

        public User? GetUserFromToken(string token)
        {
            try
            {
                if (isTesting)
                {
                    return _userRepository.GetUserByUsername(token.Split("-")[0]);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var usernameClaim = tokenDescriptor?.Claims?.FirstOrDefault(claim => claim.Type == "Username");
                var username = usernameClaim?.Value;
                string sessionId = tokenDescriptor?.Claims?.FirstOrDefault(claim => claim.Type == "SessionId")?.Value ?? "";

                if (username == null) return null;

                User? user = _userRepository.GetUserByUsername(username);

                if (user == null) return null;

                Session? currentUserSession = _sessionRepository.GetSessionById(sessionId);

                if (currentUserSession == null) return null;

                user.Session = currentUserSession;

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting user from token: {ex.Message}");
                return null;
            }
        }


        public bool ValidateToken(string token)
        {

            if (isTesting)
            {
                return _userRepository.GetUserByUsername(token.Split("-")[0]) != null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string GenerateToken(User user)
        {
            if (isTesting)
            {
                return user.Username + "-mtcgToken";
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Username", user.Username.ToString()),
                    new Claim(ClaimTypes.Role, user.Username == "admin" ? "Admin" : "User"),
                    new Claim("SessionId", user.Session.SessionId)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
