using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;


namespace MonsterTradingCardsGame.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly string _secret;
        private bool _isTesting;

        public AuthenticationService(IUserRepository userRepository, ISessionRepository sessionRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;

            // Read configuration settings from appsettings.json
            bool.TryParse(configuration["IsTesting"], out _isTesting);
            _secret = configuration.GetSection("SecretKey")?.Value ?? string.Empty;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Hashes a password using SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string hashedPassword = Convert.ToBase64String(hashBytes);
                return hashedPassword;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Verifies if a given password matches the hashed password.
        /// </summary>
        /// <param name="password">The plaintext password.</param>
        /// <param name="hashedPassword">The hashed password for comparison.</param>
        /// <returns>True if the passwords match, otherwise false.</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Verifies a user's credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if credentials are valid, otherwise false.</returns>
        public bool VerifyCredentials(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user != null)
            {
                return VerifyPassword(password, user.Password);
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves a user from a given JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The user if token is valid, otherwise null.</returns>
        public User? GetUserFromToken(string token)
        {
            try
            {
                if (_isTesting)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validates a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>True if the token is valid, otherwise false.</returns>
        public bool ValidateToken(string token)
        {

            if (_isTesting)
            {
                return _userRepository.GetUserByUsername(token.Split("-")[0]) != null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="user">The user for whom the token is being generated.</param>
        /// <returns>The generated JWT token.</returns>
        public string GenerateToken(User user)
        {
            if (_isTesting)
            {
                return user.Username + "-mtcgToken";
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
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
