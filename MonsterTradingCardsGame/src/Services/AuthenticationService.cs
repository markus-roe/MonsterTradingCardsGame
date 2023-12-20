using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
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
        private readonly string secret;

        public AuthenticationService(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
            secret = "sEcrEtKey!1234567890abcdefghijklmnopqrstuvwx";

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
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var username = tokenDescriptor?.Claims?.ToArray()[0]?.Value;

            if (username == null) return null;

            return _userRepository.GetUserByUsername(username);
        }


        public bool ValidateToken(string token)
        {
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Role, user.Username == "admin" ? "Admin" : "User")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
