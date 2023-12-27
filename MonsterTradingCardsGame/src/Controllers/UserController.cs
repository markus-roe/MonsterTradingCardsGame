using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Utils.UserStats;
using MonsterTradingCardsGame.Utils.UserProfile;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService authService;
        private readonly ICardRepository _cardRepository;

        public UserController(IUserRepository userRepository, IAuthenticationService authService, ICardRepository cardRepository)
        {
            this._userRepository = userRepository;

            this.authService = authService;
            this._cardRepository = cardRepository;
        }
        public class UserCredentials
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [Route("GET", "/users/:username")]
        public void GetUserByUsername(IHttpServerEventArguments httpEventArguments)
        {
            if (!httpEventArguments.Parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            User? user = httpEventArguments.User;

            try
            {
                if (username != httpEventArguments.User.Username || httpEventArguments.User.Username == "admin")
                {
                    httpEventArguments.Reply(403, "Forbidden: You are not allowed to access this resource.");
                    return;
                }

                UserProfile userProfile = new()
                {
                    Name = user.Name,
                    Bio = user.Bio,
                    Image = user.Image
                };

                var response = JsonSerializer.Serialize(userProfile);
                httpEventArguments.Reply(200, response);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("POST", "/users")]
        public void RegisterUser(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                var userCredentials = JsonSerializer.Deserialize<UserCredentials>(httpEventArguments.Payload);

                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    httpEventArguments.Reply(400, "Invalid username or password.");
                    return;
                }

                if (_userRepository.GetUserByUsername(userCredentials.Username) != null)
                {
                    httpEventArguments.Reply(409, "Username already exists.");
                    return;
                }

                var newUser = new User
                {
                    Username = userCredentials.Username,
                    Password = userCredentials.Password,
                };

                _userRepository.SaveUser(newUser);
                httpEventArguments.Reply(201, "User registered successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("PUT", "/users/:username")]
        public void UpdateUser(IHttpServerEventArguments httpEventArguments)
        {


            if (!httpEventArguments.Parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            if (username != httpEventArguments.User.Username && httpEventArguments.User.Username != "admin")
            {
                httpEventArguments.Reply(403, "Forbidden: You are not allowed to access this resource.");
                return;
            }

            if (string.IsNullOrEmpty(httpEventArguments.Payload))
            {
                httpEventArguments.Reply(400, "Bad Request: Request body is missing or empty.");
                return;
            }

            try
            {

                var userInfo = JsonSerializer.Deserialize<UserProfile>(httpEventArguments.Payload);
                if (userInfo == null)
                {
                    httpEventArguments.Reply(400, "Invalid user data.");
                    return;
                }

                // Check if Name, Bio, and Image exist
                if (string.IsNullOrEmpty(userInfo.Name) ||
                    string.IsNullOrEmpty(userInfo.Bio) ||
                    string.IsNullOrEmpty(userInfo.Image))
                {
                    httpEventArguments.Reply(400, "Invalid user data: Name, Bio, and Image are required.");
                    return;
                }

                var existingUser = _userRepository.GetUserByUsername(username);
                if (existingUser == null)
                {
                    httpEventArguments.Reply(404, "User not found.");
                    return;
                }

                // Update user details
                existingUser.Name = userInfo.Name;
                existingUser.Bio = userInfo.Bio;
                existingUser.Image = userInfo.Image;

                _userRepository.UpdateUser(existingUser);
                httpEventArguments.Reply(200, "User updated successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("POST", "/sessions")]
        public void Login(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                var userCredentials = JsonSerializer.Deserialize<UserCredentials>(httpEventArguments.Payload);

                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    httpEventArguments.Reply(400, "Invalid request.");
                    return;
                }

                if (authService.VerifyCredentials(userCredentials.Username, userCredentials.Password))
                {
                    var user = _userRepository.GetUserByUsername(userCredentials.Username);
                    var token = authService.GenerateToken(user);
                    httpEventArguments.Reply(200, token);
                }
                else
                {
                    httpEventArguments.Reply(401, "Invalid username/password provided");
                }
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }


        [Route("GET", "stats")]
        public void GetStats(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                User user = httpEventArguments.User;

                var stats = _userRepository.GetStatsByUser(user);

                if (stats == null)
                {
                    httpEventArguments.Reply(404, "No stats available for this user");
                    return;
                }

                httpEventArguments.Reply(200, stats);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }

        }

        [Route("GET", "scoreboard")]
        public void GetScoreboard(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                List<UserStats> scoreboard = _userRepository.GetScoreboard();

                if (scoreboard == null)
                {
                    httpEventArguments.Reply(404, "No users available");
                    return;
                }

                httpEventArguments.Reply(200, JsonSerializer.Serialize(scoreboard));
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
